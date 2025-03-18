using System;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

[GlobalClass]
public partial class AbilityCastIndicator : Node2D
{
    public static AbilityCastIndicator Instance { get; private set; }

    private Color _circleAreaColor = new(0.294f, 0.588f, 0.953f);
    private Color _circleOutlineColor = Colors.Black;

    private DrawMode _drawMode;
    private double _startRadius;
    private double _endRadius;

    private BaseChargedAbility _ability;
    private Vector2 _origin;

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
        SetProcess(false);
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

    private void DrawDirection()
    {
    }

    private void DrawCircle()
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
                DrawCircle();
                break;
            case DrawMode.Direction:
                DrawDirection();
                break;
        }
    }
}

public enum DrawMode
{
    Direction,
    Circle,
}

public record IndicatorConfig(
    DrawMode DrawMode,
    double StartRadius,
    double EndRadius)
{
    public DrawMode DrawMode = DrawMode;
    public double StartRadius = StartRadius;
    public double EndRadius = EndRadius;
}