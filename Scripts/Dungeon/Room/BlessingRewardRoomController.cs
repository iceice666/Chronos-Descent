using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Blessing;
using ChronosDescent.Scripts.Entities;
using ChronosDescent.Scripts.Items;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

/// <summary>
///     Controller for the Temporal Blessing reward room
/// </summary>
[GlobalClass]
public partial class BlessingRewardRoomController : Node
{
    // Standard setup is 3 blessings to choose from
    private const int StandardBlessingCount = 3;
    private int _collectedBlessings;

    private List<Blessing> _currentBlessings = [];
    private TimeDeity _currentDeity;
    private Label _deityLabel;

    private Player _player;

    public PackedScene BlessingItemScene { get; set; } = GD.Load<PackedScene>("res://Scenes/items/blessing_item.tscn");
    [Export] public Node2D[] BlessingPositions { get; set; } = [];

    public override void _Ready()
    {
        // Get player reference
        _player = GetNode<Player>("/root/Dungeon/Player");

        // Get the deity label
        _deityLabel = GetNode<Label>("DeityLabel");

        // Find blessing positions
        var positions = GetTree().GetNodesInGroup("BlessingPosition").Cast<Node2D>().ToArray();


        // Subscribe to events
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Subscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Unsubscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
    }

    private void OnRoomEntered()
    {
        // Reset collected blessings count
        _collectedBlessings = 0;

        // Select a random deity to offer blessings from
        SelectRandomDeity();

        // Generate and display blessing options
        GenerateBlessingOptions();

        // Set up UI
        UpdateUI();

        // Publish blessing offer event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingOfferStarted, _currentDeity);
    }

    private void OnBlessingSelected(Blessing blessing)
    {
        // Increment counter
        _collectedBlessings++;

        // If player has collected at least one blessing, allow them to leave
        if (_collectedBlessings < 1) return;
        // Show notification
        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            "Blessing received. Proceed to the next room.");

        // Allow room to be completed
        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
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
        // Clear any existing blessing items
        foreach (var child in GetChildren().Where(c => c is BlessingItem).ToList()) child.QueueFree();

        _currentBlessings.Clear();

        // Get dungeon level to adjust blessing rarity
        var dungeonLevel = DungeonManager.Instance.Level;

        // Get blessings from registry
        // 70% chance to offer deity-specific blessings
        if (GD.Randf() < 0.7f && Registry.Instance != null)
            _currentBlessings = Registry.Instance.GetBlessingsByDeity(_currentDeity, StandardBlessingCount);
        // 30% chance for random blessings
        else if (Registry.Instance != null)
            _currentBlessings = Registry.Instance.GetRandomBlessings(StandardBlessingCount, dungeonLevel);

        // If registry isn't available, create some sample blessings
        if (_currentBlessings.Count == 0) CreateSampleBlessings();

        // Calculate the number of blessing items to place
        var blessingCount = Mathf.Min(_currentBlessings.Count, BlessingPositions?.Length ?? 0);

        if (blessingCount == 0) return;


        // Create blessing items at the positions
        for (var i = 0; i < blessingCount; i++)
            CreateBlessingItem(_currentBlessings[i], BlessingPositions![i].GlobalPosition);
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
    ///     Create an interactive blessing item
    /// </summary>
    private void CreateBlessingItem(Blessing blessing, Vector2 position)
    {
        // Use custom blessing item scene if available, otherwise create simple node

        var item = BlessingItemScene != null ? BlessingItemScene.Instantiate<BlessingItem>() : new BlessingItem();


        item.GlobalPosition = position;
        item.SetBlessing(blessing);
        AddChild(item);
    }

    /// <summary>
    ///     Update the UI with deity information
    /// </summary>
    private void UpdateUI()
    {
        // Set deity label if available
        if (_deityLabel == null) return;
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
}