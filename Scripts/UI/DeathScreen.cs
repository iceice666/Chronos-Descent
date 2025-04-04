using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class DeathScreen : Control
{
    private Label _damageCausedValue;
    private Label _enemiesDefeatedValue;
    private Label _levelReachedValue;
    private Button _quitButton;
    private Button _restartButton;
    private Label _timePlayedValue;

    public override void _Ready()
    {
        Visible = false;

        // Get references to child nodes
        _enemiesDefeatedValue = GetNode<Label>("Panel/StatsContainer/EnemiesDefeatedValue");
        _damageCausedValue = GetNode<Label>("Panel/StatsContainer/DamageCausedValue");
        _levelReachedValue = GetNode<Label>("Panel/StatsContainer/LevelReachedValue");
        _timePlayedValue = GetNode<Label>("Panel/StatsContainer/TimePlayedValue");
        _restartButton = GetNode<Button>("Panel/ButtonContainer/RestartButton");
        _quitButton = GetNode<Button>("Panel/ButtonContainer/QuitButton");

        // Connect signals
        _restartButton.Pressed += OnRestartPressed;
        _quitButton.Pressed += OnQuitPressed;

        // Subscribe to the player death event
        GlobalEventBus.Instance.Subscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);

        ApplyTranslation();
    }

    public override void _ExitTree()
    {
        _restartButton.Pressed -= OnRestartPressed;
        _quitButton.Pressed -= OnQuitPressed;
        GlobalEventBus.Instance.Unsubscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }

    private void OnEntityDied(BaseEntity entity)
    {
        if (!entity.IsInGroup("Player")) return;

        Show();
    }

    private new void Show()
    {
        // Pause the game
        GetTree().Paused = true;
        GameStats.Instance.PauseTimer();

        // Update statistics
        UpdateStats();

        // Show the death screen
        Visible = true;

        // Clean up remaining enemies
        CleanupEnemies();
    }

    private void UpdateStats()
    {
        var stats = GameStats.Instance;

        _enemiesDefeatedValue.Text = stats.EnemiesDefeated.ToString();
        _damageCausedValue.Text = $"{stats.DamageCaused:F1}";
        _levelReachedValue.Text = stats.CurrentLevel.ToString();
        _timePlayedValue.Text = stats.FormatTimePlayed();
    }

    private void CleanupEnemies()
    {
        var enemies = GetTree().GetNodesInGroup("Enemy");
        foreach (var enemy in enemies) enemy.QueueFree();
    }

    private void OnRestartPressed()
    {
        // Reset game stats
        GameStats.Instance.Reset();

        // Unpause the game
        GetTree().Paused = false;

        // Reset and restart the game
        GetTree().ChangeSceneToFile("res://Scenes/prepare_room.tscn");

        // Hide the death screen
        Visible = false;
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void ApplyTranslation()
    {
        var pathKeyPairs = new[]
        {
            (Path: "Panel/TitleLabel", Type: typeof(Label), Key: "Dead_Title"),
            (Path: "Panel/StatsLabel", Type: typeof(Label), Key: "Dead_Stats"),
            (Path: "Panel/StatsContainer/EnemiesDefeatedLabel", Type: typeof(Label), Key: "Dead_EnemiesDefeated"),
            (Path: "Panel/StatsContainer/DamageCausedLabel", Type: typeof(Label), Key: "Dead_DamageCaused"),
            (Path: "Panel/StatsContainer/LevelReachedLabel", Type: typeof(Label), Key: "Dead_LevelReached"),
            (Path: "Panel/StatsContainer/TimePlayedLabel", Type: typeof(Label), Key: "Dead_TimePlayed"),
            (Path: "Panel/ButtonContainer/RestartButton", Type: typeof(Button), Key: "UI_Restart"),
            (Path: "Panel/ButtonContainer/QuitButton", Type: typeof(Button), Key: "UI_Quit")
        };

        foreach (var pair in pathKeyPairs)
            if (pair.Type == typeof(Label))
                GetNode<Label>(pair.Path).Text = Tr(pair.Key);
            else if (pair.Type == typeof(Button)) GetNode<Button>(pair.Path).Text = Tr(pair.Key);
    }
}