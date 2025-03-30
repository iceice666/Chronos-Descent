using Godot;

namespace ChronosDescent.Scripts.Core.Damage;

public enum DamageType
{
    Normal,
    Critical,
    Poison,
    Healing,
    Explosive
}

public partial class Indicator : Node2D
{
    private double _currentTime;
    private double _duration = 1.0f;
    private Label _label;
    private Vector2 _velocity = new(0, -100);

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
    }

    public override void _Process(double delta)
    {
        // Move indicator
        GlobalPosition += _velocity * (float)delta;

        // Slow down over time
        _velocity *= 0.95f;

        // Track time
        _currentTime += delta;

        // Fade out as we approach duration
        if (_currentTime > _duration * 0.5f)
        {
            var alpha = 1.0f - (_currentTime - _duration * 0.5f) / (_duration * 0.5f);
            _label.Modulate = new Color(_label.Modulate.R, _label.Modulate.G, _label.Modulate.B, (float)alpha);
        }

        // Remove when time is up
        if (_currentTime >= _duration) QueueFree();
    }

    private void Initialize(double amount, DamageType type = DamageType.Normal)
    {
        // Apply formatting based on the damage type
        switch (type)
        {
            case DamageType.Normal:
                _label.Text = $"-{amount:F0}";
                _label.Modulate = new Color(1.0f, 1.0f, 1.0f); // White
                break;

            case DamageType.Critical:
                _label.Text = $"-{amount:F0}!";
                _label.Modulate = new Color(1.0f, 0.5f, 0.0f); // Orange
                _label.Scale *= 1.2f;
                break;

            case DamageType.Poison:
                _label.Text = $"-{amount:F0}";
                _label.Modulate = new Color(0.2f, 0.8f, 0.2f); // Green
                break;

            case DamageType.Healing:
                _label.Text = $"+{amount:F0}";
                _label.Modulate = new Color(0.0f, 1.0f, 0.5f); // Cyan/Green
                break;
            
            case DamageType.Explosive:
                _label.Text = $"-{amount:F0}";
                _label.Modulate = new Color(1.0f, 0.0f, 0.0f); // Red
                break;
        }

        // Add a small random offset to the position
        GlobalPosition += new Vector2(GD.Randf() * 20 - 10, GD.Randf() * 10 - 5);
    }


    private static readonly PackedScene IndicatorScene = GD.Load<PackedScene>("res://Scenes/ui/damage_indicator.tscn");

    public static void Spawn(Node2D root, double amount, DamageType type = DamageType.Normal)
    {
        var indicator = IndicatorScene.Instantiate<Indicator>();
        root.GetNode("/root").AddChild(indicator);
        indicator.GlobalPosition = root.GetNodeOrNull<Node2D>("DamageIndicatorAnchor")?.GlobalPosition ??
                                   root.GlobalPosition - new Vector2(0, 20);
        indicator.Initialize(amount, type);
    }
}