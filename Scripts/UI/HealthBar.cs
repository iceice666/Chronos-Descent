using ChronosDescent.Scripts.node;
using Godot;
using Player = ChronosDescent.Scripts.node.Entities.Player;

namespace ChronosDescent.Scripts.UI;

public partial class HealthBar : Node2D
{
    private ProgressBar _bar;
    private Entity _entity;

    private double _hideTimer;
    private double _prevHealthPercentage = 1.0;

    [Export]
    public bool ShowForEnemies { get; set; } = true;

    [Export]
    public bool HideAtFullHealth { get; set; } = true;

    [Export]
    public double HideDelay { get; set; } = 2.0;

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("ProgressBar");
        SetProcess(false);
    }

    public void Initialize(Entity entity)
    {
        _entity = entity;
        _entity.Stats.HealthStatsChanged += OnHealthStatsChanged;

        // Initial state
        UpdateHealthBar();

        // Hide if this is an enemy and we're at full health
        if (_entity is not Player && HideAtFullHealth && _prevHealthPercentage >= 1.0)
            _bar.Visible = false;

        Visible = false;

        SetProcess(true);
    }

    private void OnHealthStatsChanged()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (_entity == null)
            return;

        var healthPercentage = _entity.Stats.Health / _entity.Stats.MaxHealth;
        healthPercentage = Mathf.Clamp(healthPercentage, 0.0, 1.0);

        if (healthPercentage > 0.97)
            return;

        if (!Visible)
            Visible = true;

        // Set progress bar value
        _bar.Value = healthPercentage;

        // Show the bar when health changes
        if (!_bar.Visible || healthPercentage < _prevHealthPercentage)
        {
            _bar.Visible = true;
            _hideTimer = HideDelay;
        }

        _prevHealthPercentage = healthPercentage;
    }

    public override void _Process(double delta)
    {
        if (_entity == null)
            return;

        // Update position to stay above the entity
        GlobalPosition = _entity.GlobalPosition;

        // Handle hiding the health bar for enemies when at full health
        if (!HideAtFullHealth || _entity is Player || !_bar.Visible || _prevHealthPercentage < 1.0)
            return;
        _hideTimer -= delta;
        if (_hideTimer <= 0)
            _bar.Visible = false;
    }

    public override void _ExitTree()
    {
        if (_entity != null)
            _entity.Stats.HealthStatsChanged -= OnHealthStatsChanged;
    }
}
