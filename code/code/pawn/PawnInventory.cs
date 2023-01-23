using System;
using System.Linq;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public partial class PawnInventory : BaseInventory
{
	public PawnInventory(Pawn owner) : base(owner) { }
	
	

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		return !IsCarryingType( entity.GetType() );
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
			return false;

		if ( IsCarryingType( entity.GetType() ) )
			return false;

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type type )
	{
		return List.Any( x => x?.GetType() == type );
	}

	public override bool Drop( Entity entity )
	{
		if ( !Game.IsServer )
			return false;

		if ( !Contains( entity ) )
			return false;

		if ( entity is BaseCarriable carriable )
		{
			carriable.OnCarryDrop(Owner);
		}

		return entity.Parent == null;
	}
}
