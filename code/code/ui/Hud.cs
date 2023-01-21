using Sandbox;
using Sandbox.UI;

namespace POLYGONWARE.ProjectOne;

public partial class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.StyleSheet.Load( "/code/ui/styles/Hud.scss" );
		RootPanel.AddChild<MouseCursor>();
	}
}
