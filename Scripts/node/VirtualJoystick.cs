using Godot;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// A virtual joystick component that supports multitouch, allowing multiple joysticks 
/// to be used simultaneously on touchscreen devices.
/// </summary>
[GlobalClass]
public partial class VirtualJoystick : Control
{
    #region Export variables

    [Export] public JoystickMovementMode MovementMode { get; set; } = JoystickMovementMode.Fixed;
    [Export] public JoystickVisibilityMode VisibilityMode { get; set; } = JoystickVisibilityMode.Always;

    [Export] public Color PressedColor { get; set; } = Colors.Gray;

    #endregion

    #region Public

    /// <summary>
    /// Whether the joystick is currently being pressed/touched
    /// </summary>
    public bool IsPressed { get; private set; }

    /// <summary>
    /// The current output vector, normalized with magnitude between 0-1
    /// </summary>
    public Vector2 Output { get; private set; }

    #endregion

    #region Enum

    public enum JoystickMovementMode
    {
        Fixed, // The joystick doesn't move.
        Dynamic, // Every time the joystick area is pressed, the joystick position is set on the touched position.
        Following // When the finger moves outside the joystick area, the joystick will follow it.
    }

    public enum JoystickVisibilityMode
    {
        Always, // Always visible
        WhenTouched // Visible only when touched
    }

    #endregion

    #region Private variables

    private TextureRect _ring;
    private TextureRect _knob;
    private int _touchIndex = -1;

    private Vector2 _knobDefaultPosition;
    private Vector2 _ringDefaultPosition;
    private Color _defaultColor;

    #endregion

    #region Lifecycle methods

    public override void _Ready()
    {
        if (VisibilityMode == JoystickVisibilityMode.WhenTouched)
            Hide();

        _knob = GetNode<TextureRect>("Knob");
        _ring = GetNode<TextureRect>("Ring");

        _knobDefaultPosition = _knob.Position;
        _ringDefaultPosition = _ring.Position;
        _defaultColor = _knob.Modulate;
    }

    public override void _Input(InputEvent @event)
    {
        // Early return if the event is not relevant
        if (@event is not (InputEventScreenTouch or InputEventScreenDrag))
            return;

        switch (@event)
        {
            // Handle touch events
            case InputEventScreenTouch touchEvent:
            {
                if (touchEvent.Pressed)
                {
                    // Handle touch press
                    HandleTouchPress(touchEvent);
                }
                else if (touchEvent.Index == _touchIndex)
                {
                    // Handle touch release for tracked touch
                    Reset();
                    if (VisibilityMode == JoystickVisibilityMode.WhenTouched)
                    {
                        Hide();
                    }

                    GetViewport().SetInputAsHandled();
                }

                return;
            }
            // Handle drag events - only process if it's the tracked touch
            case InputEventScreenDrag dragEvent when dragEvent.Index == _touchIndex:
                UpdateJoystick(dragEvent.Position);
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    #endregion

    public void Reset()
    {
        IsPressed = false;
        Output = Vector2.Zero;
        _touchIndex = -1;
        _knob.Modulate = _defaultColor;
        _knob.Position = _knobDefaultPosition;
        _ring.Position = _ringDefaultPosition;
    }

    #region Private methods

    private void HandleTouchPress(InputEventScreenTouch touchEvent)
    {
        var eventPos = touchEvent.Position;

        // Check if this is a valid touch to start tracking
        var isValidTouch = _touchIndex == -1 && IsPointInsideJoystickArea(eventPos);
        if (!isValidTouch)
            return;

        var shouldActivate = MovementMode is JoystickMovementMode.Dynamic or JoystickMovementMode.Following ||
                             (MovementMode == JoystickMovementMode.Fixed && IsPointInsideRing(eventPos));

        if (!shouldActivate)
            return;

        // Adjust joystick position for dynamic modes
        if (MovementMode is JoystickMovementMode.Dynamic or JoystickMovementMode.Following)
        {
            MoveRing(eventPos);
        }

        // Show joystick if needed
        if (VisibilityMode == JoystickVisibilityMode.WhenTouched)
        {
            Show();
        }

        // Update tracking and visual state
        _touchIndex = touchEvent.Index;
        _knob.Modulate = PressedColor;
        UpdateJoystick(eventPos);
        GetViewport().SetInputAsHandled();
    }

    private void MoveRing(Vector2 newPosition)
    {
        _ring.GlobalPosition = newPosition - _ring.PivotOffset * GetGlobalTransformWithCanvas().Scale;
    }

    private void MoveKnob(Vector2 newPosition)
    {
        _knob.GlobalPosition = newPosition - _knob.PivotOffset * _ring.GetGlobalTransformWithCanvas().Scale;
    }

    private Vector2 GetRingRadius()
    {
        return _ring.Size * _ring.GetGlobalTransformWithCanvas().Scale / 2;
    }

    private bool IsPointInsideJoystickArea(Vector2 point)
    {
        var scale = GetGlobalTransformWithCanvas().Scale;

        var x = point.X >= GlobalPosition.X && point.X <= GlobalPosition.X + (Size.X * scale.X);
        var y = point.Y >= GlobalPosition.Y && point.Y <= GlobalPosition.Y + (Size.Y * scale.Y);
        return x && y;
    }

    private bool IsPointInsideRing(Vector2 point)
    {
        var ringRadius = GetRingRadius();
        var center = _ring.GlobalPosition + ringRadius;
        var vector = point - center;

        return vector.LengthSquared() <= ringRadius.X * ringRadius.X;
    }

    private void UpdateJoystick(Vector2 touchPosition)
    {
        var baseRadius = GetRingRadius();
        var center = _ring.GlobalPosition + baseRadius;
        var vector = (touchPosition - center).LimitLength(100);


        if (MovementMode == JoystickMovementMode.Following && touchPosition.DistanceSquaredTo(center) > 100 * 100)
        {
            MoveRing(touchPosition - vector);
        }

        MoveKnob(center + vector);

        if (vector.LengthSquared() > 0)
        {
            IsPressed = true;
            Output = vector;
        }
        else
        {
            IsPressed = false;
            Output = Vector2.Zero;
        }
    }

    #endregion
}