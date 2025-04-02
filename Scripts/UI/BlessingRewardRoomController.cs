using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Blessing;
using ChronosDescent.Scripts.Dungeon;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Controller for the Temporal Blessing reward room
/// </summary>
[GlobalClass]
public partial class BlessingRewardRoomController : Control
{
    // Standard setup is 3 blessings to choose from
    private const int StandardBlessingCount = 3;
    private Control _blessingContainer;

    private List<Blessing> _currentBlessings = [];
    private TimeDeity _currentDeity;
    private Label _deityLabel;

    private Player _player;
    private Label _titleLabel;
    [Export] public PackedScene BlessingButtonScene { get; set; }

    [Export]
    public NodePath BlessingContainerPath { get; set; } =
        "CenterContainer/Panel/MarginContainer/VBoxContainer/BlessingContainer";

    [Export]
    public NodePath TitleLabelPath { get; set; } = 
        "CenterContainer/Panel/MarginContainer/VBoxContainer/TitleLabel";

    [Export]
    public NodePath DeityLabelPath { get; set; } = 
        "CenterContainer/Panel/MarginContainer/VBoxContainer/DeityLabel";

    public override void _Ready()
    {
        // Get UI nodes
        _blessingContainer = GetNode<Control>(BlessingContainerPath);
        _titleLabel = GetNode<Label>(TitleLabelPath);
        _deityLabel = GetNode<Label>(DeityLabelPath);

        // Get player reference
        _player = GetNode<Player>("/root/Dungeon/Player");

        // Subscribe to room start event
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
    }

    private void OnRoomEntered()
    {
        // Clear any previous blessings
        ClearBlessingOptions();

        // Select a random deity to offer blessings from
        SelectRandomDeity();

        // Generate and display blessing options
        GenerateBlessingOptions();

        // Set up UI
        UpdateUI();

        // Publish blessing offer event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingOfferStarted, _currentDeity);
    }

    /// <summary>
    ///     Clear all blessing options from the container
    /// </summary>
    private void ClearBlessingOptions()
    {
        foreach (var child in _blessingContainer.GetChildren()) child.QueueFree();

        _currentBlessings.Clear();
    }

    /// <summary>
    ///     Select a random deity to offer blessings from
    /// </summary>
    private void SelectRandomDeity()
    {
        // For variety, we weight deity selection based on player's existing blessings
        // Players should encounter all deities over time

        var deityWeights = new Dictionary<TimeDeity, float>
        {
            { TimeDeity.Chronos, 1.0f },
            { TimeDeity.Aion, 1.0f },
            { TimeDeity.Kairos, 1.0f },
            { TimeDeity.Aevum, 1.0f }
        };

        // Lower weight for deities the player already has blessings from
        foreach (var blessing in _player.BlessingManager.GetAllBlessings())
            if (deityWeights.ContainsKey(blessing.Deity))
                deityWeights[blessing.Deity] *= 0.7f;

        // Select a random deity based on weights
        var totalWeight = deityWeights.Values.Sum();

        var roll = GD.Randf() * totalWeight;
        var current = 0f;

        _currentDeity = TimeDeity.Chronos; // Default

        foreach (var pair in deityWeights)
        {
            current += pair.Value;
            if (roll > current) continue;
            _currentDeity = pair.Key;
            break;
        }
    }

    /// <summary>
    ///     Generate and display blessing options
    /// </summary>
    private void GenerateBlessingOptions()
    {
        // Get dungeon level to adjust blessing rarity
        var dungeonLevel = 1; // Default if not found
        if (DungeonManager.Instance != null) dungeonLevel = DungeonManager.Instance.Level;

        // Get blessings from registry
        // We can either get from a specific deity or random ones

        // 70% chance to offer deity-specific blessings
        if (GD.Randf() < 0.7f && Registry.Instance != null)
            _currentBlessings = Registry.Instance.GetBlessingsByDeity(_currentDeity, StandardBlessingCount);
        // 30% chance for random blessings
        else if (Registry.Instance != null)
            _currentBlessings = Registry.Instance.GetRandomBlessings(StandardBlessingCount, dungeonLevel);

        // If registry isn't available, create some sample blessings
        if (_currentBlessings.Count == 0) CreateSampleBlessings();

        // Create blessing buttons
        foreach (var blessing in _currentBlessings) CreateBlessingButton(blessing);
    }

    /// <summary>
    ///     Create fallback blessing options if registry isn't available
    /// </summary>
    private void CreateSampleBlessings()
    {
        // Create sample blessings for testing
        _currentBlessings.Add(new TemporalEchoBlessing());
        _currentBlessings.Add(new ProbabilityFieldBlessing());
        _currentBlessings.Add(new TimelineEfficiencyBlessing());
    }

    /// <summary>
    ///     Create a button for a blessing option
    /// </summary>
    private void CreateBlessingButton(Blessing blessing)
    {
        // If we have a custom button scene, use it
        var button = BlessingButtonScene != null
            ? BlessingButtonScene.Instantiate<BlessingButton>()
            : new BlessingButton();

        _blessingContainer.AddChild(button);

        // Configure the button with blessing
        button.SetBlessing(blessing);

        // Connect to blessing selected event
        button.BlessingSelected += OnBlessingSelected;
    }

    /// <summary>
    ///     Update the UI with deity information
    /// </summary>
    private void UpdateUI()
    {
        // Set title
        _titleLabel.Text = "Temporal Blessings";

        // Set deity label
        _deityLabel.Text = Blessing.GetDeityName(_currentDeity);

        // Set deity color
        var deityTheme = new Color(1, 1, 1); // Default white

        switch (_currentDeity)
        {
            case TimeDeity.Chronos:
                deityTheme = new Color(0.3f, 0.7f, 1f); // Blue
                break;
            case TimeDeity.Aion:
                deityTheme = new Color(0.7f, 0.3f, 0.9f); // Purple
                break;
            case TimeDeity.Kairos:
                deityTheme = new Color(1f, 0.6f, 0.2f); // Orange
                break;
            case TimeDeity.Aevum:
                deityTheme = new Color(0.2f, 0.8f, 0.5f); // Green
                break;
        }

        _deityLabel.Modulate = deityTheme;
    }

    /// <summary>
    ///     Handle blessing selection
    /// </summary>
    private void OnBlessingSelected(Blessing blessing)
    {
        // Apply the blessing to the player
        _player.BlessingManager.AddBlessing(blessing);

        // Notify the system about selection
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingSelected, blessing);

        // Close the blessing selection UI
        Visible = false;

        // Allow the player to move and open doors
        _player.Moveable = true;

        // Open doors by triggering a room cleared event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
    }
}