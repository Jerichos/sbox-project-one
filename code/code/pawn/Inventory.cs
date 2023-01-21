using Sandbox;

namespace POLYGONWARE.ProjectOne;

public class Inventory : IBaseInventory
{	
	public Entity Active { get; }
	
	public Inventory( Pawn player )
	{
	}

	public void OnChildAdded( Entity child )
	{
		throw new System.NotImplementedException();
	}

	public void OnChildRemoved( Entity child )
	{
		throw new System.NotImplementedException();
	}

	public void DeleteContents()
	{
		throw new System.NotImplementedException();
	}

	public int Count()
	{
		throw new System.NotImplementedException();
	}

	public Entity GetSlot( int i )
	{
		throw new System.NotImplementedException();
	}

	public int GetActiveSlot()
	{
		throw new System.NotImplementedException();
	}

	public bool SetActiveSlot( int i, bool allowempty )
	{
		throw new System.NotImplementedException();
	}

	public bool SwitchActiveSlot( int idelta, bool loop )
	{
		throw new System.NotImplementedException();
	}

	public Entity DropActive()
	{
		throw new System.NotImplementedException();
	}

	public bool Drop( Entity ent )
	{
		throw new System.NotImplementedException();
	}

	
	public bool SetActive( Entity ent )
	{
		throw new System.NotImplementedException();
	}

	public bool Add( Entity ent, bool makeactive = false )
	{
		throw new System.NotImplementedException();
	}

	public bool Contains( Entity ent )
	{
		throw new System.NotImplementedException();
	}
}
