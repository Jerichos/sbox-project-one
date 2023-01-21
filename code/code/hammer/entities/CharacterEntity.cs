using Editor;
using Sandbox;

namespace POLYGONWARE.ProjectOne.hammer;

/// <summary>
/// Character entity.
/// </summary>
[Library( "character" ), HammerEntity]
[Title( "Character" ), Category( "Character" ), Icon( "place" )]
public partial class CharacterEntity : Pawn
{
	/// <summary>
	/// Help text in hammer
	/// </summary>
	[Property( Title = "Start Disabled" ) ]
	public bool StartDisabled { get; set; } = false;
}
