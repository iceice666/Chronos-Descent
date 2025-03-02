using Godot;

namespace ChronosDescent.Scripts.node;

public partial class VirtualJoystick : Node2D
{
    public bool IsPressed { get; private set; }
    public Vector2 Output => GetOutput();

    private float _maxLength = 100;
    private float _maxLengthSquared;

    private Node2D _knob;
    private Button _button;

    private void OnButtonPressed()
    {
        IsPressed = true;
    }

    private void OnButtonReleased()
    {
        IsPressed = false;
    }

    private Vector2 GetOutput()
    {
        if (!IsPressed)
            return Vector2.Zero;


        return _knob.GlobalPosition - GlobalPosition / _maxLength;
    }

    public override void _Ready()
    {
        _knob = GetNode<Node2D>("Knob");
        _button = GetNode<Button>("Button");

        _button.Connect(BaseButton.SignalName.ButtonDown, Callable.From(OnButtonPressed));
        _button.Connect(BaseButton.SignalName.ButtonUp, Callable.From(OnButtonReleased));

        _maxLength *= Scale.X;
        _maxLengthSquared = _maxLength * _maxLength;
    }

    public override void _PhysicsProcess(double delta)
    {
        var mousePos = GetGlobalMousePosition();

        if (IsPressed)
        {
            if (mousePos.DistanceSquaredTo(GlobalPosition) <= _maxLengthSquared)
            {
                _knob.GlobalPosition = mousePos;
            }
            else
            {
                // Calculate direction vector from center to mouse
                var direction = mousePos - GlobalPosition;

                // Normalize and scale by maximum length
                direction = direction.Normalized() * _maxLength;

                // Set knob position
                _knob.GlobalPosition = GlobalPosition + direction;
            }
        }
        else
        {
            _knob.GlobalPosition = _knob.GlobalPosition.Lerp( GlobalPosition, (float)delta * 5);
        }
    }
}