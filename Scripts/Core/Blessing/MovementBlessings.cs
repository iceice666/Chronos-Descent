using ChronosDescent.Scripts.Core.State;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Phase Shifting: Dash through objects and enemies
/// </summary>
[GlobalClass]
public partial class PhaseShiftingBlessing : Blessing
{
    public override string Id { get; protected set; } = "phase_shifting";
    public override string Title { get; protected set; } = "Phase Shifting";

    public override string Description { get; protected set; } =
        "Dash ability allows you to phase through obstacles and enemies.{0}";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Rare;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Movement;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Kairos;

    [Export] public bool DealsDamage { get; set; } // Level 2+ deals damage
    [Export] public double PhaseDamage { get; set; } = 10.0; // Damage when phasing through enemies

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    public override void OnApply()
    {
        // Set description based on level
        if (CurrentLevel == 1)
        {
            Description = string.Format(Description, "");
            DealsDamage = false;
        }
        else
        {
            Description = string.Format(Description, " Deal damage to enemies you phase through.");
            DealsDamage = true;
        }
    }

    public override void OnLevelUp()
    {
        // Update description based on level
        if (CurrentLevel >= 2)
        {
            Description = string.Format(Description, " Deal damage to enemies you phase through.");
            DealsDamage = true;
        }
    }

    public override void OnAbilityUsed(AbilityVariant abilityVariant)
    {
        // Only process dash ability
        if (abilityVariant == AbilityVariant.Dash)
        {
            // In a real implementation, this would modify the dash ability's behavior
            // to allow passing through obstacles

            // For demonstration, just notify the player
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                "Phase Shifting activated!");

            // At level 2+, it would also create a damage area during the dash
            if (DealsDamage)
                // Simplified damage notification
                GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                    "Phase damage dealt!");
        }
    }
}

/// <summary>
///     Time Slipstream: Increased movement speed after using abilities
/// </summary>
[GlobalClass]
public partial class TimeSlipstreamBlessing : Blessing
{
    private bool _boostActive;

    private double _boostTimer;
    public override string Id { get; protected set; } = "time_slipstream";
    public override string Title { get; protected set; } = "Time Slipstream";

    public override string Description { get; protected set; } =
        "After using an ability, gain {0}% movement speed for {1} seconds.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Uncommon;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Movement;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aion;

    [Export] public double SpeedBoostPercent { get; set; } = 30.0; // 30% speed boost
    [Export] public double BoostDuration { get; set; } = 3.0; // Duration in seconds

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnApply()
    {
        // Set up multiplicative modifier for move speed
        MultiplicativeModifiers = new Dictionary<StatFieldSpecifier, double>();

        // Format description with current values
        Description = string.Format(Description,
            SpeedBoostPercent * CurrentLevel,
            BoostDuration);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description,
            SpeedBoostPercent * CurrentLevel,
            BoostDuration);
    }

    public override void OnAbilityUsed(AbilityVariant abilityVariant)
    {
        // Any ability use triggers the speed boost
        _boostTimer = BoostDuration;

        if (_boostActive) return;
        _boostActive = true;

        // Apply the speed boost
        var boost = 1.0 + SpeedBoostPercent * CurrentLevel / 100.0;
        MultiplicativeModifiers[StatFieldSpecifier.MoveSpeed] = boost;

        // Update stats
        if (Owner?.BlessingManager != null)
            Owner.StatsManager?.Recalculate(
                new System.Collections.Generic.Dictionary<StatFieldSpecifier, double>(),
                Utils.ToDictionary(MultiplicativeModifiers));

        // Notify player
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
            "Time Slipstream activated!");
    }

    public override void OnTick(double delta)
    {
        // Update boost timer if active
        if (!_boostActive) return;
        _boostTimer -= delta;

        if (!(_boostTimer <= 0)) return;
        // Deactivate the boost
        _boostActive = false;

        // Reset the modifier
        MultiplicativeModifiers[StatFieldSpecifier.MoveSpeed] = 1.0;

        // Update stats
        if (Owner?.BlessingManager != null)
            Owner.StatsManager?.Recalculate(
                new System.Collections.Generic.Dictionary<StatFieldSpecifier, double>(),
                Utils.ToDictionary(MultiplicativeModifiers));

        // Notify player
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
            "Time Slipstream ended");
    }
}

/// <summary>
///     Spacetime Compression: Shorter distance between rooms
/// </summary>
[GlobalClass]
public partial class SpacetimeCompressionBlessing : Blessing
{
    public override string Id { get; protected set; } = "spacetime_compression";
    public override string Title { get; protected set; } = "Spacetime Compression";
    public override string Description { get; protected set; } = "The number of rooms per level is reduced by {0}%.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Common;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Movement;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    [Export] public double RoomReductionPercent { get; set; } = 15.0; // 15% fewer rooms

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    // This blessing would affect dungeon generation parameters
    // In a real implementation, it would modify the DungeonGenerator

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description, RoomReductionPercent * CurrentLevel);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description, RoomReductionPercent * CurrentLevel);
    }

    // Method for DungeonGenerator to query room reduction
    public double GetRoomReductionFactor()
    {
        return 1.0 - RoomReductionPercent * CurrentLevel / 100.0;
    }
}

/// <summary>
///     Temporal Wake: Leave damaging trails when moving quickly
/// </summary>
[GlobalClass]
public partial class TemporalWakeBlessing : Blessing
{
    private const double TrailSpawnRate = 0.25; // How often to spawn trail segments
    private bool _isMovingFast;

    private Vector2 _lastPosition = Vector2.Zero;

    private double _trailTimer;
    public override string Id { get; protected set; } = "temporal_wake";
    public override string Title { get; protected set; } = "Temporal Wake";

    public override string Description { get; protected set; } =
        "Leave a damaging trail behind you when moving at full speed. Trail deals {0} damage per second.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Epic;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Movement;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aevum;

    [Export] public double WakeDamagePerSecond { get; set; } = 8.0; // Base damage per second
    [Export] public double WakeDuration { get; set; } = 1.5; // How long the wake lasts

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description, WakeDamagePerSecond * CurrentLevel);

        // Initialize last position
        if (Owner != null) _lastPosition = Owner.GlobalPosition;
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description, WakeDamagePerSecond * CurrentLevel);
    }

    public override void OnTick(double delta)
    {
        if (Owner == null) return;

        // Update trail timer
        _trailTimer -= delta;

        // Check if player is moving fast enough
        var currentPosition = Owner.GlobalPosition;
        var movementSpeed = (_lastPosition - currentPosition).Length() / (float)delta;
        _lastPosition = currentPosition;

        // Consider "fast" to be at least 75% of max speed
        var speedThreshold = (float)(Owner.StatsManager.MoveSpeed * 0.75);
        _isMovingFast = movementSpeed >= speedThreshold;

        // Spawn trail segments if moving fast and timer is up
        if (!_isMovingFast || !(_trailTimer <= 0)) return;
        // Reset timer
        _trailTimer = TrailSpawnRate;

        // In a real implementation, this would create a temporary damage area
        // at the player's position

        // Simplified notification
        // Only show occasionally to avoid spam
        if (GD.RandRange(0, 3) == 0)
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                "Temporal Wake active");
    }

    // Get the current damage for the wake
    public double GetWakeDamage()
    {
        return WakeDamagePerSecond * CurrentLevel;
    }
}