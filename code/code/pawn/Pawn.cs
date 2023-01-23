using System.Numerics;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public partial class Pawn : AnimatedEntity
{
	[Net] public float SprintSpeed { get; set; } = 320.0f;
	[Net] public float WalkSpeed { get; set; } = 150.0f;
	
	[Net, Predicted]
	public PawnController Controller { get; set; }
	
	// I guess it's active item in hands?
	[Net, Predicted] public Entity ActiveChild { get; set; }
	[ClientInput] public Entity ActiveChildInput { get; set; }

	private readonly ClothingContainer Clothing = new();
	public readonly IBaseInventory Inventory;

	[Net, Predicted] 
	public Vector3 LookAt { get;  set; }

	public Pawn()
	{
		Inventory = new PawnInventory(this);
	}

	public Pawn( IClient client ) : this()
	{
		Clothing.LoadFromClient(client);
	}
	
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
		
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = false;
		EnableShadowInFirstPerson = true;
		
		Camera.FirstPersonViewer = null;
	}
	
	public void Respawn()
	{
		Clothing.DressEntity( this );

		Inventory.Add( new Pistol(), true );
	}
	
	// An example BuildInput method within a player's Pawn class.
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Vector3 MouseDirection { get; set; }
	[ClientInput] public Vector3 MouseOrigin { get; set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	
	private bool _panCamera;
	private float _zoomT;
	private float _cameraDistance;

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

		_zoomT -= Input.MouseWheel * 0.2f;
		_zoomT = float.Clamp( _zoomT, 0, 1 );

		MouseDirection = Screen.GetDirection(MouseCursor.Instance.Position);
		MouseOrigin = Camera.Position;
		
		ActiveChild?.BuildInput();
		//Log.Info("MousePosition: " + MouseCursor.Instance.Position + " Direction: " + MouseDirection);
	}

	private Entity lastWeapon;
	
	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		
		if ( ActiveChildInput.IsValid() && ActiveChildInput.Owner == this )
		{
			ActiveChild = ActiveChildInput;
		}
		
		// WSAD input normalized
		var moveDirection = InputDirection.Normal;
		
		Rotation = Rotation.FromYaw(ViewAngles.yaw);
		moveDirection *= Rotation;

		// apply some speed to it
		Velocity = moveDirection * (Input.Down( InputButton.Run ) ? SprintSpeed : WalkSpeed);

		// apply it to our position using MoveHelper, which handles collision
		// detection and sliding across surfaces for us
		
		PawnMove helper = new PawnMove( Position, Velocity );
		if ( helper.MyTryMove(Time.Delta) > 0 )
		{
			Position = helper.Position;
		}

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );
		animHelper.WithWishVelocity( Velocity );
		animHelper.WithVelocity( Velocity );
		
		if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

		if ( ActiveChild is BaseCarriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
		
		
		// by default rotate character to mouse position
		var mouseRay = Trace.Ray(MouseOrigin, MouseOrigin + MouseDirection * 1000).Run();
		if ( mouseRay.Hit )
		{
			var lookAt = Rotation.LookAt( (Position - mouseRay.HitPosition).Normal );
			var forward = lookAt.Forward;
			
			var lookAtPosition = mouseRay.HitPosition + Vector3.Down * 55; // WHyyyyyyyyyy???????
			
			DebugOverlay.Sphere(lookAtPosition, 5, Color.Red);

			animHelper.WithLookAt(lookAtPosition);
		}
		
		SimulateActiveChild(cl, ActiveChild);
		
		// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
		if ( Game.IsServer && Input.Pressed( InputButton.PrimaryAttack ) )
		{
			if ( ActiveChild is Pistol pistol )
			{
				//Log.Info("Pistol");
				//pistol.AttackPrimary();
			}
			
			
			// hit ground or objects with ray coming from "mouse position"
			if ( mouseRay.Hit )
			{
				// Log.Info("!!! HIT " + mouseRay.HitPosition + " mouseDirection: " + MouseDirection);
				// DebugOverlay.TraceResult(mouseRay, 1f);
			}
		}
	}
	
	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		
		// Top Down Camera
		
		var maxRange = 250;
		var minRange = 50;
		var range = minRange + _zoomT * maxRange;

		Camera.Rotation = ViewAngles.ToRotation();

		var rot = Camera.Rotation;

		_cameraDistance = MathX.Lerp(_cameraDistance, range * Scale, 10f * Time.Delta);

		Camera.Position = Position + Vector3.Up * 64 + rot.Forward * -_cameraDistance;
	}

	public override void OnChildAdded( Entity child )
	{
		Inventory?.OnChildAdded( child );
	}

	public override void OnChildRemoved( Entity child )
	{
		Inventory?.OnChildRemoved( child );
	}
	
	/// <summary>
	/// This isn't networked, but it's predicted. If it wasn't then when the prediction system
	/// re-ran the commands LastActiveChild would be the value set in a future tick, so ActiveEnd
	/// and ActiveStart would get called multiple times and out of order, causing all kinds of pain.
	/// </summary>
	[Predicted]
	Entity LastActiveChild { get; set; }

	/// <summary>
	/// Simulated the active child. This is important because it calls ActiveEnd and ActiveStart.
	/// If you don't call these things, viewmodels and stuff won't work, because the entity won't
	/// know it's become the active entity.
	/// </summary>
	public virtual void SimulateActiveChild( IClient cl, Entity child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		if ( LastActiveChild.IsAuthority )
		{
			LastActiveChild.Simulate( cl );
		}
	}

	/// <summary>
	/// Called when the Active child is detected to have changed
	/// </summary>
	public virtual void OnActiveChildChanged( Entity previous, Entity next )
	{
		if ( previous is BaseCarriable previousBc )
		{
			previousBc?.ActiveStop( this, previousBc.Owner != this );
		}

		if ( next is BaseCarriable nextBc )
		{
			nextBc?.ActiveStart( this );
		}
	}
}
