using Sandbox;

namespace POLYGONWARE.ProjectOne;

[Spawnable]
[Library( "weapon_pistol", Title = "Pistol" )]
public partial class Pistol : Weapon
{
	public readonly IWeaponData WeaponData = new PistolData();
	
	public TimeSince TimeSinceDischarge { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( WeaponData.ModelPath );
		
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed(InputButton.PrimaryAttack);
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		(Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
		
		ShootEffects();
		PlaySound( WeaponData.SoundPath );
		ShootBullet(WeaponData.Spread, WeaponData.Force, WeaponData.Damage, WeaponData.BulletSize);
	}

	public void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;
		
		ShootEffects();
		PlaySound( WeaponData.SoundPath );
		ShootBullet(pos, rot.Forward, WeaponData.Spread, WeaponData.Force, WeaponData.Damage, WeaponData.BulletSize);

		ApplyAbsoluteImpulse(rot.Backward * 200.0f);
	}

	// Looks like if collide with pistol it shoots?
	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f )
		{
			Discharge();
		}
	}
}
