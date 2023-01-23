using System;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public struct PawnMove
{
	public Vector3 Position;
	public Vector3 Velocity;
	public float MaxStandableAngle;

	public PawnMove( Vector3 position, Vector3 velocity, params string[] solidTags ) : this()
	{
		Velocity = velocity;
		Position = position;
	}

	public PawnMove( Vector3 position, Vector3 velocity ) : this( position, velocity, "solid", "playerclip", "passbullets", "player" )
	{

	}

	public float MyTryMove(float deltaTime)
	{
		float travelFraction = 1;

		BBox box = new BBox( new Vector3( -15, -15, 1 ), new Vector3( 15, 15, 50 ) );

		var tr = Trace.Box( box, Position, Position + Velocity * deltaTime )
			.WithAnyTags( "solid" ).Run();

		if ( tr.Hit )
		{
			var dot = Vector3.Dot(tr.Direction, tr.Normal);
			var o = tr.Direction - (tr.Normal * dot);
			float damp = (1 + dot) * 3f;
			damp = damp.Clamp( 0, 1 );

			Log.Info("Hit " + tr.Normal + " o " + o.Normal + " dot " + dot + " damp: " + damp);
			DebugOverlay.Line(tr.HitPosition, tr.HitPosition + o.Normal * 100, 3, true);
			
			// backoff a bit from collision
			Position = tr.EndPosition + tr.Normal * 0.031235f;

			var velocityDelta = o.Normal * Velocity.Length * deltaTime * damp;
			
			var tr2 = Trace.Box( box, Position, Position + velocityDelta )
				.WithAnyTags( "solid" ).Run();
			
			if ( tr2.Hit )
			{
				Log.Info("Tr2 hit");
				
				// backoff a bit from collision again
				Position = tr2.EndPosition + tr2.Normal * 0.031235f;
			}
			else
			{
				Position += velocityDelta;
			}

			return 1;
		}
		else
			Position += Velocity * deltaTime;
		
		return travelFraction;
	}


	/// <summary>
	/// Return true if this is the trace is a floor. Checks hit and normal angle.
	/// </summary>
	public bool IsFloor( TraceResult tr )
	{
		if ( !tr.Hit ) return false;
		return tr.Normal.Angle( Vector3.Up ) < MaxStandableAngle;
	}

	/// <summary>
	/// Apply an amount of friction to the velocity
	/// </summary>
	public void ApplyFriction( float frictionAmount, float delta )
	{
		float StopSpeed = 100.0f;

		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return;

		newspeed /= speed;
		Velocity *= newspeed;
	}
}
