using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Chrono Harvester: Enemies drop more chronoshards
/// </summary>
[GlobalClass]
public partial class ChronoHarvesterBlessing : Blessing
{
    public override string Id { get; protected set; } = "chrono_harvester";
    public override string Title { get; protected set; } = "Chrono Harvester";
    public override string Description { get; protected set; } = "Enemies drop {0}% more chronoshards when defeated.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Common;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Utility;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aevum;

    [Export] public double ChronoshardBoostPercent { get; set; } = 20.0; // 20% more chronoshards

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    // Store the original boost value
    private double GetTotalBoost => ChronoshardBoostPercent * CurrentLevel / 100.0;

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description, ChronoshardBoostPercent * CurrentLevel);

        // Register for enemy death events
        GlobalEventBus.Instance.Subscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }

    public override void OnRemove()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description, ChronoshardBoostPercent * CurrentLevel);
    }

    private void OnEntityDied(BaseEntity entity)
    {
        // Only process if this is an enemy
        if (entity.IsInGroup("Enemy"))
        {
            // Calculate bonus shards (would be integrated with the actual drop system)
            // This is a simplified implementation for demonstration
            var baseDropAmount = GD.RandRange(1, 5);
            var bonusFactor = 1.0 + GetTotalBoost;
            var totalDropAmount = (int)(baseDropAmount * bonusFactor);

            // Broadcast an effect if there's a significant bonus
            if (totalDropAmount > baseDropAmount)
                GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                    $"Chrono Harvester: +{totalDropAmount - baseDropAmount} shards");
        }
    }
}

/// <summary>
///     Timeline Efficiency: Faster cooldown on abilities
/// </summary>
[GlobalClass]
public partial class TimelineEfficiencyBlessing : Blessing
{
    public override string Id { get; protected set; } = "timeline_efficiency";
    public override string Title { get; protected set; } = "Timeline Efficiency";
    public override string Description { get; protected set; } = "Reduces all ability cooldowns by {0}%.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Uncommon;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Utility;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    [Export] public double CooldownReductionPercent { get; set; } = 15.0; // 15% cooldown reduction

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    // Cooldown reduction is implemented at the ability level in real usage
    // This blessing would provide a global modifier that ability cooldowns check against

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description, CooldownReductionPercent * CurrentLevel);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description, CooldownReductionPercent * CurrentLevel);
    }

    // Get the cooldown reduction multiplier (e.g., 0.85 for 15% reduction)
    public double GetCooldownMultiplier()
    {
        return 1.0 - CooldownReductionPercent * CurrentLevel / 100.0;
    }
}

/// <summary>
///     Temporal Insight: Reveals room layouts and enemy positions
/// </summary>
[GlobalClass]
public partial class TemporalInsightBlessing : Blessing
{
    public override string Id { get; protected set; } = "temporal_insight";
    public override string Title { get; protected set; } = "Temporal Insight";

    public override string Description { get; protected set; } =
        "Reveals the layout of rooms and shows enemy positions {0}.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Rare;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Utility;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aion;

    [Export] public bool RevealsThroughWalls { get; set; } // Level 2 reveals through walls

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    public override void OnApply()
    {
        // Format description based on level
        if (CurrentLevel == 1)
        {
            Description = string.Format(Description, "when in line of sight");
            RevealsThroughWalls = false;
        }
        else
        {
            Description = string.Format(Description, "even through walls");
            RevealsThroughWalls = true;
        }

        // Subscribe to room entered event to reveal layout
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
    }

    public override void OnRemove()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
    }

    public override void OnLevelUp()
    {
        // Update description based on level
        if (CurrentLevel >= 2)
        {
            Description = string.Format(Description, "even through walls");
            RevealsThroughWalls = true;
        }
    }

    private void OnRoomEntered()
    {
        // In a real implementation, this would trigger the minimap to fully reveal
        // and potentially highlight enemy positions

        // Simplified notification
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
            "Room layout revealed by Temporal Insight!");
    }
}

/// <summary>
///     Resource Recursion: Chance to not consume consumable items
/// </summary>
[GlobalClass]
public partial class ResourceRecursionBlessing : Blessing
{
    public override string Id { get; protected set; } = "resource_recursion";
    public override string Title { get; protected set; } = "Resource Recursion";
    public override string Description { get; protected set; } = "{0}% chance to not consume items when used.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Epic;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Utility;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Kairos;

    [Export] public double RecursionChancePercent { get; set; } = 25.0; // 25% chance to not consume

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description, RecursionChancePercent * CurrentLevel);

        // In a full implementation, we would subscribe to item use events
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description, RecursionChancePercent * CurrentLevel);
    }

    // Method to check if an item should be consumed
    public bool ShouldConsumeItem()
    {
        // Roll for non-consumption
        return GD.RandRange(0, 100) >= RecursionChancePercent * CurrentLevel;
    }
}