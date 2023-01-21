using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace POLYGONWARE.ProjectOne;

[StyleSheet( "/code/ui/styles/MouseCursor.scss" )]
public class MouseCursor : Panel
{
	private bool _update;
	private Vector2 _lastMousePosition;
	private Vector2 _mousePositionRaw;
	private Vector2 _mousePosition;
	public static MouseCursor Instance { get; private set; }

	private Image _image { get; set; }

	public MouseCursor() : base()
	{
		_image = Add.Image( "", "cursor" );
		_image.SetTexture( "ui/cursor.png" );
		
		var mousePosition = Mouse.Position / Screen.Size;

		Instance = this;
	}

	public override void Tick()
	{
		if ( !_update )
			return;

		_lastMousePosition += Mouse.Delta;

		_lastMousePosition.x = float.Clamp( _lastMousePosition.x, 0, Screen.Size.x - 16);
		_lastMousePosition.y = float.Clamp( _lastMousePosition.y, 0, Screen.Size.y - 16);
		
		_mousePosition = _lastMousePosition  / Screen.Size;
		_image.Style.Left = Length.Fraction( _mousePosition.x );
		_image.Style.Top = Length.Fraction( _mousePosition.y );

		_image.Style.Dirty();
		base.Tick();
	}

	public Vector2 Position => _lastMousePosition;

	public void Disable()
	{
		if(!_update)
			return;

		//_lastMousePosition = Mouse.Position;
		_image.Style.Display = DisplayMode.None;

		_update = false;
	}

	public void Enable()
	{
		if(_update)
			return;
		//_lastMousePosition = Mouse.Position;
		_image.Style.Display = DisplayMode.Flex;
		
		_update = true;
	}
}
