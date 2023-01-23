using System.Collections.Generic;
using Sandbox;

namespace POLYGONWARE.ProjectOne;

public class BaseInventory : IBaseInventory
{
	public Entity Owner { get; init; } // init - once object is set, it can't be changed...
	public List<Entity> List = new();

	public virtual Entity Active
	{
		get
		{
			return (Owner as Pawn)?.ActiveChild;
		}
		set
		{
			if ( Owner is Pawn pawn )
			{
				pawn.ActiveChild = value;
			}
		}
	}

	public BaseInventory( Entity owner )
	{
		Owner = owner;
	}

	public virtual bool CanAdd( Entity entity )
	{
		if ( entity is BaseCarriable carriable && carriable.CanCarry( Owner ) )
			return true;

		return false;
	}

	public virtual void DeleteContents()
	{
		Game.AssertServer();

		foreach ( var item in List.ToArray() )
		{
			item.Delete();
		}
		
		List.Clear();
	}

	public virtual Entity GetSlot( int slotID )
	{
		if ( List.Count <= slotID ) return null;
		if ( slotID < 0 ) return null;

		return List[slotID];
	}

	public virtual int Count() => List.Count;

	public virtual int GetActiveSlot()
	{
		var activeEntity = Active;
		var count = Count();

		for ( int i = 0; i < count; i++ )
		{
			if ( List[i] == activeEntity )
				return i;
		}

		return -1;
	}

	public virtual void Pickup( Entity entity )
	{
		// TODO? WHY?
	}

	public virtual void OnChildAdded(Entity child)
	{
		if(!CanAdd(child))
			return;
		
		if(List.Contains(child))
			return;
		
		List.Add(child);
	}

	public virtual void OnChildRemoved( Entity child )
	{
		if ( List.Remove( child ) )
		{
			Log.Info("Child removed from inventory " + child.Name + " className: " + child.ClassName);
		}
	}

	public virtual bool SetActiveSlot( int slotid, bool evenIfEmpty = false )
	{
		var entity = GetSlot( slotid );
		if ( Active == entity )
			return false;

		if ( !evenIfEmpty && entity == null )
			return false;

		Active = entity;

		return entity.IsValid();
	}

	public virtual bool SwitchActiveSlot( int idelta, bool loop )
	{
		var count = Count();
		if ( count == 0 ) return false;

		var slot = GetActiveSlot();
		var nextSlot = slot + idelta;

		if ( loop )
		{
			while ( nextSlot < 0 ) nextSlot += count;
			while ( nextSlot >= count ) nextSlot -= count;
		}
		else
		{
			if ( nextSlot < 0 ) return false;
			if ( nextSlot >= count ) return false;
		}

		return SetActiveSlot( nextSlot, false );
	}

	public virtual Entity DropActive()
	{
		if ( !Game.IsServer ) return null;

		var active = Active;
		if ( active == null ) return null;

		if ( Drop( active ) )
		{
			Active = null;
			return active;
		}

		return null;
	}

	public virtual bool Drop( Entity entity )
	{
		if ( !Game.IsServer )
			return false;

		if ( !Contains( entity ) )
			return false;

		entity.Parent = null;
		
		if(entity is BaseCarriable carriable)
			carriable.OnCarryDrop(Owner);

		return true;
	}

	public virtual bool Contains( Entity entity )
	{
		return List.Contains( entity );
	}

	public virtual bool SetActive( Entity entity )
	{
		if ( Active == entity ) return false;
		if ( !Contains( entity ) ) return false;

		Active = entity;
		return true;
	}

	public virtual bool Add( Entity entity, bool makeActive = false )
	{
		Game.AssertServer();

		// What if I'm owner
		if ( entity.Owner != null )
			return false;

		if ( !CanAdd( entity ) )
			return false;

		if ( entity is not BaseCarriable carriable )
			return false;

		if ( !carriable.CanCarry( Owner ) )
			return false;

		entity.Parent = Owner;
		
		carriable.OnCarryStart(Owner);

		if ( makeActive )
		{
			SetActive( entity );
		}

		return true;
	}
}
