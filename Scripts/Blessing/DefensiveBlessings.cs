using ChronosDescent.Scripts.Core.Damage;
using Godot;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Temporal Shield: Automatically rewind time when taking fatal damage (once per room)
/// </summary>
[GlobalClass]
public partial class TemporalShieldBlessing : Blessing
{
    private bool _isAvailable = true;

    public TemporalShieldBlessing()
    {
        // Format description with current values
        Description = TranslationManager.TrFormat("Blessing_TemporalShield_Desc", HealthRestoredPercent * CurrentLevel);
    }

    public override void OnApply()
    {
        // Reset availability when entering a new room
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomEntered, OnRoomStarted);
    }

    public override string Id { get; protected set; } = "Blessing_TemporalShield";
    public override string Title { get; protected set; } = TranslationManager.Tr("Blessing_TemporalShield");

    public override string Description { get; protected set; }

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Epic;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Defensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    [Export] public double HealthRestoredPercent { get; set; } = 25.0; // 25% health restored

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    public override void OnRemove()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomEntered, OnRoomStarted);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = TranslationManager.TrFormat("Blessing_TemporalShield_Desc", HealthRestoredPercent * CurrentLevel);
    }

    public override void OnDamageTaken(double amount)
    {
        // Check if this damage would kill the player
        if (_isAvailable && Owner.StatsManager.Health - amount <= 0)
        {
            // Prevent death
            _isAvailable = false;

            // Restore health
            var healthToRestore = Owner.StatsManager.MaxHealth * (HealthRestoredPercent * CurrentLevel / 100.0);
            Owner.StatsManager.Health = healthToRestore;

            // Visual effect
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                TranslationManager.Tr("Notification_TemporalShield_Activated"));

            // Trigger time rewind effect
            Owner.BlessingManager.NotifyTimeRewound();
        }
    }

    private void OnRoomStarted()
    {
        // Reset shield for new room
        _isAvailable = true;
    }
}

/// <summary>
///     Probability Field: Chance to avoid damage completely
/// </summary>
[GlobalClass]
public partial class ProbabilityFieldBlessing : Blessing
{
    private const double CooldownDuration = 0.5; // Cooldown between avoidance checks

    private double _cooldownTimer;
    public override string Id { get; protected set; } = "Blessing_ProbabilityField";
    public override string Title { get; protected set; } = TranslationManager.Tr("Blessing_ProbabilityField");

    public override string Description { get; protected set; }

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Rare;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Defensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Kairos;

    [Export] public double AvoidChance { get; set; } = 10.0; // 10% chance to avoid damage

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnApply()
    {
        // Format description with current values
        Description = TranslationManager.TrFormat("Blessing_ProbabilityField_Desc", AvoidChance * CurrentLevel);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = TranslationManager.TrFormat("Blessing_ProbabilityField_Desc", AvoidChance * CurrentLevel);
    }

    public override void OnDamageTaken(double amount)
    {
        // Only check for avoidance if cooldown is over
        if (_cooldownTimer <= 0)
            // Check if damage is avoided
            if (GD.RandRange(0, 100) < AvoidChance * CurrentLevel)
            {
                // Negate the damage by healing for the same amount
                Owner.TakeDamage(amount, DamageType.Healing);

                // Visual effect
                GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
                    TranslationManager.Tr("Notification_Damage_Avoided"));

                // Start cooldown
                _cooldownTimer = CooldownDuration;
            }
    }

    public override void OnTick(double delta)
    {
        // Update cooldown timer
        if (_cooldownTimer > 0) _cooldownTimer -= delta;
    }
}

/// <summary>
///     Wound Reversal: Recover health when using time rewind ability
/// </summary>
[GlobalClass]
public partial class WoundReversalBlessing : Blessing
{
    public WoundReversalBlessing()
    {
        // Format description with current values
        Description = TranslationManager.TrFormat("Blessing_WoundReversal_Desc", HealthRecoveryPercent * CurrentLevel);
    }

    public override string Id { get; protected set; } = "Blessing_WoundReversal";
    public override string Title { get; protected set; } = TranslationManager.TrFormat("Blessing_WoundReversal");

    public override string Description { get; protected set; }

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Uncommon;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Defensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    [Export] public double HealthRecoveryPercent { get; set; } = 15.0; // 15% of max health

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = TranslationManager.TrFormat("Blessing_WoundReversal_Desc", HealthRecoveryPercent * CurrentLevel);
    }

    public override void OnTimeRewound()
    {
        // Calculate health to restore
        var healthToRestore = Owner.StatsManager.MaxHealth * (HealthRecoveryPercent * CurrentLevel / 100.0);

        // Heal the player
        Owner.TakeDamage(healthToRestore, DamageType.Healing);

        // Visual effect
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
            TranslationManager.TrFormat("Notification_WoundReversal", healthToRestore.ToString("F1")));
    }
}

/// <summary>
///     Timeline Diversion: Redirect damage to time-delayed clones
/// </summary>
[GlobalClass]
public partial class TimelineDiversionBlessing : Blessing
{
    private const double CooldownDuration = 3.0; // Cooldown between diversions

    private double _cooldownTimer;

    public TimelineDiversionBlessing()
    {
        // Format description with current values
        Description =
            TranslationManager.TrFormat("Blessing_TimelineDiversion_Desc", DiversionChance, DamageReductionPercent);
    }

    public override string Id { get; protected set; } = "Blessing_TimelineDiversion";
    public override string Title { get; protected set; } = TranslationManager.TrFormat("Blessing_TimelineDiversion");

    public override string Description { get; protected set; }

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Legendary;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Defensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aevum;

    [Export] public double DiversionChance { get; set; } = 35.0; // 35% chance to divert
    [Export] public double DamageReductionPercent { get; set; } = 50.0; // 50% of damage diverted

    public override bool IsStackable { get; protected set; } = false;

    public override void OnApply()
    {
    }

    public override void OnDamageTaken(double amount)
    {
        // Only check for diversion if cooldown is over
        if (!(_cooldownTimer <= 0)) return;
        // Check if diversion occurs
        if (!(GD.RandRange(0, 100) < DiversionChance)) return;

        // Calculate diverted damage
        var divertedAmount = amount * (DamageReductionPercent / 100.0);

        // Reduce the damage by healing for the diverted amount
        Owner.TakeDamage(divertedAmount, DamageType.Healing);

        // Create visual effect
        var cloneEffect = new Node2D();
        cloneEffect.Name = "TimelineCloneEffect";
        Owner.AddChild(cloneEffect);

        // Clean up after a delay
        Owner.GetTree().CreateTimer(1.0).Timeout += () => cloneEffect?.QueueFree();

        // Visual notification
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle,
            TranslationManager.TrFormat("Notification_DamageDiverted", divertedAmount.ToString("F1")));

        // Start cooldown
        _cooldownTimer = CooldownDuration;
    }

    public override void OnTick(double delta)
    {
        // Update cooldown timer
        if (_cooldownTimer > 0) _cooldownTimer -= delta;
    }
}