using Godot;

namespace ChronosDescent.Scripts.Entity.UI;

public partial class DamageIndicator : Node2D
{
    public enum DamageType
    {
        Normal,
        Critical,
        Poison,
        Healing
    }

    private float _currentTime;
    private float _duration = 1.0f;
    private Label _label;
    private Vector2 _velocity = new(0, -60); // Initial upward movement

    // Called when the node enters the scene tree for the first time
    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
    }

    // Called every frame. 'Delta' is the elapsed time since the previous frame
    public override void _Process(double delta)
    {
        // Move indicator
        Position += _velocity * (float)delta;

        // Slow down over time
        _velocity *= 0.95f;

        // Track time
        _currentTime += (float)delta;

        // Fade out as we approach duration
        if (_currentTime > _duration * 0.5f)
        {
            var alpha = 1.0f - (_currentTime - _duration * 0.5f) / (_duration * 0.5f);
            _label.Modulate = new Color(_label.Modulate.R, _label.Modulate.G, _label.Modulate.B, alpha);
        }

        // Remove when time is up
        if (_currentTime >= _duration) QueueFree();
    }

    // Set the damage amount and type
    public void Setup(double amount, DamageType type = DamageType.Normal)
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
        }

        // Add a small random offset to the position
        Position += new Vector2(GD.Randf() * 20 - 10, GD.Randf() * 10 - 5);
    }
}