using Sandbox;

namespace POLYGONWARE.ProjectOne;

[Title("Carriable"), Icon("luggage")]
public class BaseCarriable : AnimatedEntity
{
	public virtual string ViewModelPath => null;
	public BaseViewModel ViewModelEntity { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();

		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		// EnableHideInFirstPerson = true;
		// EnableShadowInFirstPerson = true;
	}

	public virtual bool CanCarry( Entity carrier )
	{
		return true;
	}

	public virtual void OnCarryStart( Entity carrier )
	{
		if(Game.IsClient)
			return;
		
		SetParent(carrier, true);
		Owner = carrier;
		EnableAllCollisions = false;
		EnableDrawing = true;
	}

	public virtual void SimulateAnimator( CitizenAnimationHelper animator )
	{
		animator.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		animator.Handedness = CitizenAnimationHelper.Hand.Both;
		animator.AimBodyWeight = 1f;
	}

	public virtual void OnCarryDrop(Entity dropper)
	{
		if(Game.IsClient)
			return;
		
		SetParent(null);
		Owner = null;
		EnableDrawing = true;
		EnableAllCollisions = true;
	}

	public virtual void ActiveStart( Entity entity )
	{
		EnableDrawing = true;

		if ( IsLocalPawn )
		{
			DestroyViewModel();
			DestroyHudElements();

			CreateViewModel();
			CreateHudElements();
		}
	}

	public virtual void ActiveStop( Entity entity, bool dropped )
	{
		if ( !dropped )
		{
			EnableDrawing = false;
		}

		if ( Game.IsClient )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient && ViewModelEntity.IsValid() )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	public virtual void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
		{
			Log.Warning("ViewModelPath not set");
		}

		ViewModelEntity = new BaseViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel(ViewModelPath);
	}

	public virtual void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	public virtual void CreateHudElements()
	{
		
	}
	
	public virtual void DestroyHudElements()
	{
		
	}

	public virtual ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;
}
