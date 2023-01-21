using Sandbox;

namespace POLYGONWARE.ProjectOne;

[Library]
public partial class PawnController : BaseNetworkable
{
	[Net] public float SprintSpeed { get; set; } = 320.0f;
	[Net] public float WalkSpeed { get; set; } = 150.0f;
}
