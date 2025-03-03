using Godot;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// A virtual joystick component that supports multitouch, allowing multiple joysticks 
/// to be used simultaneously on touchscreen devices.
/// </summary>
[GlobalClass]
public partial class VirtualJoystick : Node2D
{
    #region Export Variables

    [ExportGroup("Joystick Settings")]
    [Export]
    public float MaxLength { get; set; } = 100f;

    [Export] public float DeadZone { get; set; } = 0.1f;
    [Export] public bool ReturnToCenter { get; set; } = true;
    [Export] public float ReturnSpeed { get; set; } = 10f;
    [Export] public bool DynamicPosition { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Whether the joystick is currently being pressed/touched
    /// </summary>
    public bool IsPressed { get; private set; }

    /// <summary>
    /// The current output vector, normalized with magnitude between 0-1
    /// </summary>
    public Vector2 Output => GetOutput();

    /// <summary>
    /// The touch ID currently controlling this joystick, or -1 if not active
    /// </summary>
    public int TouchIndex { get; private set; } = -1;

    #endregion

    #region Private Variables

    private Node2D _knob;
    private Control _touchArea;
    private float _maxLengthSquared;
    private Vector2 _basePosition;
    private Vector2 _initialPosition;

    #endregion

    #region Lifecycle Methods

    public override void _Ready()
    {
        _knob = GetNode<Node2D>("Knob");
        _touchArea = GetNode<Control>("TouchArea");

        _initialPosition = GlobalPosition;
        _basePosition = GlobalPosition;
        _maxLengthSquared = MaxLength * MaxLength;
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            // Handle touch events
            case InputEventScreenTouch touchEvent:
                HandleTouchEvent(touchEvent);
                break;
            // Handle touch drag events
            case InputEventScreenDrag dragEvent:
                HandleDragEvent(dragEvent);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Return to center when not pressed
        if (IsPressed || !ReturnToCenter) return;
        
        _knob.GlobalPosition = _knob.GlobalPosition.Lerp(_basePosition, (float)delta * ReturnSpeed);

        // Snap to center if very close
        if (_knob.GlobalPosition.DistanceSquaredTo(_basePosition) < 1)
        {
            _knob.GlobalPosition = _basePosition;
        }
    }

    #endregion

    #region Input Handling Methods

    private void HandleTouchEvent(InputEventScreenTouch touchEvent)
    {
        var touchPosition = touchEvent.Position;

        // Touch pressed
        if (touchEvent.Pressed)
        {
            // Only consider this touch if we're not already tracking one
            // and if the touch is within our touch area
            if (TouchIndex != -1 || !IsPositionInTouchArea(touchPosition)) return;
            
            // Start tracking this touch
            TouchIndex = touchEvent.Index;
            IsPressed = true;

            if (DynamicPosition)
            {
                // Set base position to touch position for dynamic joystick
                _basePosition = touchPosition;
                GlobalPosition = touchPosition;
            }

            // Set knob position to touch position
            UpdateKnobPosition(touchPosition);
        }
        // Touch released - only care about our tracked touch
        else if (touchEvent.Index == TouchIndex)
        {
            // Stop tracking this touch
            TouchIndex = -1;
            IsPressed = false;

            if (!DynamicPosition) return;
            
            // Reset position for dynamic joystick
            GlobalPosition = _initialPosition;
            _basePosition = _initialPosition;
        }
    }

    private void HandleDragEvent(InputEventScreenDrag dragEvent)
    {
        // Only process drag events for our tracked touch
        if (dragEvent.Index == TouchIndex)
        {
            UpdateKnobPosition(dragEvent.Position);
        }
    }

    #endregion

    #region Helper Methods

    private bool IsPositionInTouchArea(Vector2 position)
    {
        // Convert position to local coordinates of the touch area
        var localPos = _touchArea.GetGlobalTransform().AffineInverse() * position;

        // Check if the position is within the touch area's bounds
        return _touchArea.GetRect().HasPoint(localPos);
    }

    private void UpdateKnobPosition(Vector2 targetPosition)
    {
        // Calculate vector from joystick center to target position
        var direction = targetPosition - _basePosition;

        // Apply distance constraint if needed
        if (direction.LengthSquared() > _maxLengthSquared)
        {
            direction = direction.Normalized() * MaxLength;
        }

        // Update knob position
        _knob.GlobalPosition = _basePosition + direction;
    }

    /// <summary>
    /// Get the current joystick output vector
    /// </summary>
    private Vector2 GetOutput()
    {
        if (!IsPressed)
            return Vector2.Zero;

        // Calculate direction vector
        var direction = _knob.GlobalPosition - _basePosition;

        // Convert to 0 to 1 range
        var output = direction / MaxLength;

        // Apply deadzone
        var length = output.Length();
        if (length < DeadZone)
        {
            return Vector2.Zero;
        }

        // Normalize output accounting for deadzone
        if (!(length > 0)) return output;
        output = output.Normalized() * ((length - DeadZone) / (1 - DeadZone));

        // Ensure magnitude doesn't exceed 1
        if (output.Length() > 1.0f)
        {
            output = output.Normalized();
        }

        return output;
    }

    /// <summary>
    /// Reset the joystick to its initial state
    /// </summary>
    public void Reset()
    {
        IsPressed = false;
        TouchIndex = -1;
        _knob.GlobalPosition = _basePosition;
    }

    #endregion
}