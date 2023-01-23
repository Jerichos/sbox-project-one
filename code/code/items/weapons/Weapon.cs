using System.Collections.Generic;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual float ReloadTime => 3.0f;
	
	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted] 
	public TimeSince TimeSinceReload { get; set; }
	
	[Net, Predicted] 
	public bool IsReloading { get; set; }
	
	[Net, Predicted] 
	public TimeSince TimeSinceDeployed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger()
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void ActiveStart( Entity entity )
	{
		base.ActiveStart( entity );

		TimeSinceDeployed = 0;
	}

	public override void Reload()
	{
		if(IsReloading)
			return;

		TimeSinceReload = 0;
		IsReloading = true;
		
		(Owner as AnimatedEntity)?.SetAnimParameter("b_reload", true);

		StartReloadEffects();
	}

	public override void Simulate( IClient cl )
	{
		if(TimeSinceDeployed < 0.6f)
			return;
		
		if(!IsReloading)
			base.Simulate(cl);

		if ( IsReloading && TimeSinceReload > ReloadTime )
			OnReloadFinish();
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter("reload", true);
		
		// TODO: third person reload?
	}

	public override void CreateViewModel()
	{
		Game.AssertClient();
		
		if(string.IsNullOrEmpty(ViewModelPath))
			return;

		ViewModelEntity = new BaseViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};
		
		ViewModelEntity.SetModel(ViewModelPath);
	}
	
	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;
		
		user.StartTouch(this);

		return false;
	}

	public bool IsUsable( Entity user )
	{
		var player = user as Pawn;
		if ( Owner != null ) return false;

		if ( player.Inventory is PawnInventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();
		
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		
		ViewModelEntity?.SetAnimParameter("fire", true);
	}

	public override IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2 )
	{
		return base.TraceBullet( start, end, radius );
	}
	
	public virtual IEnumerable<TraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc", "glass" )
			.Ignore( this );

		var result = trace.Run();

		if ( result.Hit )
		{
			yield return result;
		}
		else
		{
			trace = trace.Size( radius );
			result = trace.Run();

			if ( result.Hit )
			{
				yield return result;
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		//forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;
		
		Log.Info("ShotBullet " + pos + " dir: " + dir + " forward: " + forward);

		foreach ( var result in TraceBullet(pos, pos + forward * 5000, bulletSize) )
		{
			result.Surface.DoBulletImpact( result );
			
			if(!Game.IsServer) continue;
			if(!result.Entity.IsValid) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( result.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( result )
					.WithAttacker( Owner )
					.WithWeapon( this );
				
				result.Entity.TakeDamage(damageInfo);
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Log.Info("shoot bullet: " + Owner.AimRay.Forward);
		Game.SetRandomSeed(Time.Tick);

		// var ray = Owner.AimRay;
		// ShootBullet(ray.Position, ray.Forward, spread, force, damage, bulletSize);
		ShootBullet(Position, Rotation.Forward, spread, force, damage, bulletSize);
	}


	/// <summary>
	/// Shoot multiple bullets from owners view point 
	/// </summary>
	public virtual void ShootBullets( int bulletCount, float spread, float force, float damage, float bulletSize )
	{
		var ray = Owner.AimRay;
		for ( int i = 0; i < bulletCount; i++ )
		{
			ShootBullet(ray.Position, ray.Forward, spread, force / bulletCount, damage, bulletSize);
		}
	}
}
