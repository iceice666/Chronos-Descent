using Godot;

namespace ChronosDescent.Scripts.Ability.UI;

[GlobalClass]
public partial class AbilityCastIndicator : Node2D
{
    private BaseChargedAbility _ability;

    private Color _circleAreaColor = new(0.294f, 0.588f, 0.953f);
    private Color _circleOutlineColor = Colors.Black;

    private DrawMode _drawMode;
    private double _endRadius;
    private Vector2 _origin;
    private double _startRadius;
    public static AbilityCastIndicator Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        //   Visible = false;
        SetProcess(false);

        Instance = this;
    }

    // Initialize the ability cast indicator
    public void Start(BaseChargedAbility ability)
    {
        var config = ability.IndicatorConfig;

        Visible = true;
        _drawMode = config.DrawMode;
        _startRadius = config.StartRadius;
        _endRadius = config.EndRadius;
        _ability = ability;
        _origin = GlobalPosition;
        SetProcess(true);
    }

    // Update the ability cast indicator
    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    // Stop the ability cast indicator
    public void Stop()
    {
        Visible = false;
        SetProcess(false);
    }

    private void DrawDirectionIndicator()
    {
    }

    private void DrawCircleIndicator()
    {
        var radius = (float)(_startRadius +
                             (_endRadius - _startRadius) * _ability.CurrentChargeTime / _ability.MaxChargeTime);

        DrawCircle(_origin, radius, _circleAreaColor);
        DrawArc(_origin, radius, 0, float.Tau, 32, _circleOutlineColor);
    }

    public override void _Draw()
    {
        switch (_drawMode)
        {
            case DrawMode.Circle:
                DrawCircleIndicator();
                break;
            case DrawMode.Direction:
                DrawDirectionIndicator();
                break;
        }
    }
}

public enum DrawMode
{
    Direction,
    Circle
}

public record IndicatorConfig(
    DrawMode DrawMode,
    double StartRadius,
    double EndRadius)
{
    public DrawMode DrawMode = DrawMode;
    public double EndRadius = EndRadius;
    public double StartRadius = StartRadius;
}