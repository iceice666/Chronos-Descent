using Godot;
using Player = ChronosDescent.Scripts.node.Entities.Player;

namespace ChronosDescent.Scripts.UI;

public partial class PlayerHealthBar : Control
{
    private ProgressBar _healthBar;
    private ProgressBar _delayedHealthBar;
    private bool _initialized;
    private Player _player;
    private double _currentHealthDisplay;
    private double _delayedHealthValue;
    private double _damageDelayTimer;
    private bool _isDelayedBarTransitioning;
    private Label _valueLabel;

    [Export]
    public double HealthBarSmoothSpeed { get; set; } = 5.0;

    [Export]
    public double DamageDelay { get; set; } = 0.5;

    [Export]
    public double DelayedBarSpeed { get; set; } = 3.0;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("ProgressBar");
        _delayedHealthBar = GetNode<ProgressBar>("DelayedProgressBar");
        _valueLabel = GetNode<Label>("ValueLabel");
        SetProcess(false);
    }

    public void Initialize(Player player)
    {
        if (_initialized)
            return;

        _player = player;
        _player.Stats.HealthStatsChanged += OnHealthStatsChanged;

        // Set initial values
        _currentHealthDisplay = _player.Stats.Health;
        _delayedHealthValue = _player.Stats.Health;
        _healthBar.MaxValue = _player.Stats.MaxHealth;
        _healthBar.Value = _player.Stats.Health;
        _delayedHealthBar.MaxValue = _player.Stats.MaxHealth;
        _delayedHealthBar.Value = _player.Stats.Health;

        UpdateValueLabel();

        _initialized = true;
        SetProcess(true);
    }

    private void OnHealthStatsChanged()
    {
        if (_player == null)
            return;

        double previousHealth = _currentHealthDisplay;

        // Update target health display to the new value
        _currentHealthDisplay = _player.Stats.Health;

        // If health decreased, start the damage delay
        if (_currentHealthDisplay < previousHealth)
        {
            _damageDelayTimer = DamageDelay;
            _isDelayedBarTransitioning = false;
        }

        // Update max values in case max health changed
        _healthBar.MaxValue = _player.Stats.MaxHealth;
        _delayedHealthBar.MaxValue = _player.Stats.MaxHealth;

        // Update the value label
        UpdateValueLabel();
    }

    private void UpdateValueLabel()
    {
        _valueLabel.Text = $"{(int)_player.Stats.Health} / {(int)_player.Stats.MaxHealth}";
    }

    public override void _Process(double delta)
    {
        if (!_initialized || _player == null)
            return;

        // Smooth health bar animation for the red bar
        _healthBar.Value = Mathf.Lerp((float)_healthBar.Value, (float)_currentHealthDisplay, (float)(delta * HealthBarSmoothSpeed));

        // Process delayed health bar behavior
        ProcessDelayedHealthBar(delta);
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
                _currentHealthDisplay,
                (float)(delta * DelayedBarSpeed * _player.Stats.MaxHealth)
            );

            _delayedHealthBar.Value = _delayedHealthValue;

            // Check if transition is complete
            if (Mathf.IsEqualApprox((float)_delayedHealthValue, (float)_currentHealthDisplay))
            {
                _isDelayedBarTransitioning = false;
            }
        }
        else
        {
            _delayedHealthBar.Value = _delayedHealthValue;
        }
    }

    public override void _ExitTree()
    {
        if (_player != null)
            _player.Stats.HealthStatsChanged -= OnHealthStatsChanged;
    }
}
