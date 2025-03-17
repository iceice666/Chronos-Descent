using ChronosDescent.Scripts.node;
using Godot;
using Player = ChronosDescent.Scripts.node.Entities.Player;

namespace ChronosDescent.Scripts.UI;

public partial class HealthBar : Node2D
{
    private ProgressBar _bar;
    private ProgressBar _delayedBar;
    private Entity _entity;

    private double _hideTimer;
    private double _prevHealthPercentage = 1.0;
    private double _delayedHealthValue = 1.0;
    private double _damageDelayTimer;
    private bool _isDelayedBarTransitioning;


    public bool HideAtFullHealth { get; set; } = true;

    [Export]
    public double HideDelay { get; set; } = 2.0;

    [Export]
    public double DamageDelay { get; set; } = 0.5;

    [Export]
    public double DamageDelaySpeed { get; set; } = 3.0;

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("ProgressBar");
        _delayedBar = GetNode<ProgressBar>("DelayedProgressBar");
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
        {
            _bar.Visible = false;
            _delayedBar.Visible = false;
        }

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

        // If health has decreased, update the damage delay timer
        if (healthPercentage < _prevHealthPercentage)
        {
            _damageDelayTimer = DamageDelay;
            _isDelayedBarTransitioning = false;

            // Make both bars visible when taking damage
            _bar.Visible = true;
            _delayedBar.Visible = true;
            _hideTimer = HideDelay;
        }

        // Set immediate progress bar value (red)
        _bar.Value = healthPercentage;

        // The delayed bar stays at previous health until delay triggers transition
        _delayedBar.Value = _delayedHealthValue;

        _prevHealthPercentage = healthPercentage;
    }

    public override void _Process(double delta)
    {
        if (_entity == null)
            return;

        // Update position to stay above the entity
        GlobalPosition = _entity.GlobalPosition;

        // Process delayed health bar behavior
        ProcessDelayedHealthBar(delta);

        // Handle hiding the health bar for enemies when at full health
        if (!HideAtFullHealth || _entity is Player || !_bar.Visible || _prevHealthPercentage < 1.0)
            return;

        _hideTimer -= delta;
        if (_hideTimer <= 0)
        {
            _bar.Visible = false;
            _delayedBar.Visible = false;
        }
    }

    private void ProcessDelayedHealthBar(double delta)
    {
        // Count down the damage delay timer
        if (_damageDelayTimer > 0)
        {
            _damageDelayTimer -= delta;

            // Start transitioning when delay expires
            if (_damageDelayTimer <= 0)
            {
                _isDelayedBarTransitioning = true;
            }
        }

        // Update the delayed health bar if it's transitioning
        if (_isDelayedBarTransitioning)
        {
            // Smoothly decrease the delayed bar value toward the actual health
            _delayedHealthValue = Mathf.MoveToward(
                _delayedHealthValue,
                _bar.Value,
                (float)(delta * DamageDelaySpeed)
            );

            _delayedBar.Value = _delayedHealthValue;

            // Check if transition is complete
            if (Mathf.IsEqualApprox((float)_delayedHealthValue, (float)_bar.Value))
            {
                _isDelayedBarTransitioning = false;
            }
        }
    }

    public override void _ExitTree()
    {
        if (_entity != null)
            _entity.Stats.HealthStatsChanged -= OnHealthStatsChanged;
    }
}
