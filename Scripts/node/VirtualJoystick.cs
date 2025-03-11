using Godot;

namespace ChronosDescent.Scripts.node;

public partial class VirtualJoystick : Control
{
    // Joystick deadzone threshold
    [Export] public float JoystickDeadzone { get; set; } = 0.2f;
    
    private Vector2 _centerPosition;
    private Sprite2D _knob;
    private float _maxRadius;

    // References to child nodes
    private Sprite2D _ring;

    // Touch tracking
    private int _touchIndex = -1;

    // Public interface
    public bool IsPressed { get; private set; }
    public Vector2 Output { get; private set; } = Vector2.Zero;

    public override void _Ready()
    {
        // Get references to child nodes
        _ring = GetNode<Sprite2D>("Ring");
        _knob = GetNode<Sprite2D>("Knob");

        // Initialize the joystick
        UpdateJoystickLayout();

        // Make sure this control can receive input events
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Notification(int what)
    {
        // If the size of the control changes, update the joystick layout
        if (what == NotificationResized) UpdateJoystickLayout();
    }

    private void UpdateJoystickLayout()
    {
        // Set center position to the center of our control
        _centerPosition = new Vector2(Size.X / 2, Size.Y / 2);

        // Position the ring and knob at the center
        _ring.Position = _centerPosition;
        _knob.Position = _centerPosition;

        // Calculate maximum radius based on control size
        _maxRadius = Mathf.Min(Size.X, Size.Y) / 2.5f;
    }

    public override void _GuiInput(InputEvent @event)
    {
        // Note: InputEvents in _GuiInput are already in control-local coordinates

        switch (@event)
        {
            // Handle touch events
            case InputEventScreenTouch touchEvent:
                HandleTouchEvent(touchEvent);
                break;
            // Handle touch drag/motion events
            case InputEventScreenDrag dragEvent:
                HandleDragEvent(dragEvent);
                break;
        }
    }

    private void HandleTouchEvent(InputEventScreenTouch touchEvent)
    {
        switch (touchEvent.Pressed)
        {
            // If it's a touch press, and we're not yet tracking a touch
            case true when _touchIndex == -1:

                // Start tracking this touch
                _touchIndex = touchEvent.Index;
                IsPressed = true;

                // Update knob position based on touch position
                UpdateKnobPosition(touchEvent.Position);


                break;
            // If it's a touch release, and it's the touch we're tracking
            case false when touchEvent.Index == _touchIndex:

                // Stop tracking this touch
                _touchIndex = -1;
                IsPressed = false;

                // Reset knob position and output
                _knob.Position = _centerPosition;
                Output = Vector2.Zero;

                break;
        }
    }

    private void HandleDragEvent(InputEventScreenDrag dragEvent)
    {
        // Only process drag events for the touch we're tracking
        if (dragEvent.Index != _touchIndex) return;


        // Update knob position based on drag position
        UpdateKnobPosition(dragEvent.Position);
    }

    private void UpdateKnobPosition(Vector2 position)
    {
        // Calculate vector from the center to touch position and clamp position to max radius
        var direction = position - _centerPosition;

        // Update knob position
        _knob.Position = _centerPosition + direction.LimitLength(_maxRadius);

        // Calculate and update output (normalized -1 to 1 range)
        Output = ApplyDeadzone(direction).LimitLength();
    }
    
    
    private Vector2 ApplyDeadzone(Vector2 input)
    {
        var length = input.Length();
        if (length < JoystickDeadzone)
            return Vector2.Zero;
        // Smoothly scale input from deadzone to 1.0
        return input.Normalized() * ((length - JoystickDeadzone) / (1.0f - JoystickDeadzone));
    }
}