using ChronosDescent.Scripts.node;
using Godot;
using Player = ChronosDescent.Scripts.node.Entities.Player;

namespace ChronosDescent.Scripts.UI;

public partial class PlayerHealthBar : Control
{
	private Player _player;
	private ProgressBar _healthBar;
	private Label _valueLabel;
	private double _targetHealthDisplay;
	private bool _initialized;

	[Export] public double SmoothSpeed { get; set; } = 5.0;

	public override void _Ready()
	{
		_healthBar = GetNode<ProgressBar>("ProgressBar");
		_valueLabel = GetNode<Label>("ProgressBar/ValueLabel");
		SetProcess(false);
	}

	public void Initialize(Player player)
	{
		if (_initialized) return;

		_player = player;
		_player.Stats.HealthStatsChanged += OnHealthStatsChanged;

		// Set initial values
		_targetHealthDisplay = _player.Stats.Health;
		_healthBar.MaxValue = _player.Stats.MaxHealth;
		_healthBar.Value = _player.Stats.Health;
		
		_valueLabel.Text = $"{(int)_player.Stats.Health} / {(int)_player.Stats.MaxHealth}";

		_initialized = true;
		SetProcess(true);
	}

	private void OnHealthStatsChanged()
	{
		if (_player == null) return;

		_targetHealthDisplay = _player.Stats.Health;
		_healthBar.MaxValue = _player.Stats.MaxHealth;
		_valueLabel.Text = $"{(int)_player.Stats.Health} / {(int)_player.Stats.MaxHealth}";
	}

	public override void _Process(double delta)
	{
		if (!_initialized || _player == null) return;

		// Smooth health bar animation
		var currentDisplay = _healthBar.Value;
		var smoothValue = Mathf.Lerp(currentDisplay, _targetHealthDisplay, delta * SmoothSpeed);
		_healthBar.Value = smoothValue;
	}

	public override void _ExitTree()
	{
		if (_player != null)
		{
			_player.Stats.HealthStatsChanged -= OnHealthStatsChanged;
		}
	}
}
