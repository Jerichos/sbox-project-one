using System.Numerics;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public partial class Pawn : AnimatedEntity
{
	[Net] public float SprintSpeed { get; set; } = 320.0f;
	[Net] public float WalkSpeed { get; set; } = 150.0f;
	
	[Net, Predicted]
	public PawnController Controller { get; set; }
	
	[Net, Predicted]
	public BBox BoxCollider { get; set; }
	
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		//
		// Use a watermelon model
		//
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}
	
	// An example BuildInput method within a player's Pawn class.
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	[ClientInput] public Vector3 MouseDirection { get; set; }
	[ClientInput] public Vector3 MouseOrigin { get; set; }
	
	private bool _panCamera;
	private float _zoomT;

	private bool PanCamera
	{
		get => _panCamera;
		set
		{
			if(_panCamera == value)
				return;
			
			if(value)
				MouseCursor.Instance.Disable();
			else
			{
				MouseCursor.Instance.Enable();
			}

			_panCamera = value;
		}
	}

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		PanCamera = Input.Down( InputButton.SecondaryAttack );

		if (PanCamera)
		{
			var look = Input.AnalogLook;
			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( 50f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;
		}

		_zoomT -= Input.MouseWheel * 0.1f;
		_zoomT = float.Clamp( _zoomT, 0, 1 );

		MouseDirection = Screen.GetDirection(MouseCursor.Instance.Position);
		MouseOrigin = Camera.Position;
		//Log.Info("MousePosition: " + MouseCursor.Instance.Position + " Direction: " + MouseDirection);
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		
		// WSAD input normalized
		var moveDirection = InputDirection.Normal;
		
		Rotation = Rotation.FromYaw(ViewAngles.yaw);
		moveDirection *= Rotation;

		// apply some speed to it
		Velocity = moveDirection * (Input.Down( InputButton.Run ) ? SprintSpeed : WalkSpeed);

		// apply it to our position using MoveHelper, which handles collision
		// detection and sliding across surfaces for us
		
		MoveHelper helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 16 );
		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Position = helper.Position;
		}

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );
		animHelper.WithWishVelocity( Velocity );
		animHelper.WithVelocity( Velocity );

		// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
		if ( Game.IsServer && Input.Pressed( InputButton.PrimaryAttack ) )
		{
			// hit ground or objects with ray coming from "mouse position"
			var mouseRay = Trace.Ray(MouseOrigin, MouseOrigin + MouseDirection * 1000).Run();

			if ( mouseRay.Hit )
			{
				Log.Info("!!! HIT " + mouseRay.HitPosition + " mouseDirection: " + MouseDirection);
				DebugOverlay.TraceResult(mouseRay, 10f);
			}
		}
	}
	
	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		
		// zoom
		var maxRange = 250;
		var minRange = 50;
		var range = minRange + _zoomT * maxRange;

		// Top Down Camera
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FirstPersonViewer = null;

		Vector3 targetPos;
		var center = Position + Vector3.Up * 64;

		var pos = center;
		var rot = Camera.Rotation /** Rotation.FromAxis( Vector3.Up, -16 )*/;

		float distance = range * Scale;
		targetPos = pos /*+ rot.Right * ((CollisionBounds.Mins.x + 32) * Scale)*/;
		targetPos += rot.Forward * -distance;

		var tr = Trace.Ray( pos, targetPos )
			.WithAnyTags( "solid" )
			.Ignore( this )
			.Radius( 8 )
			.Run();

		Camera.Position = tr.EndPosition;
	}
}
