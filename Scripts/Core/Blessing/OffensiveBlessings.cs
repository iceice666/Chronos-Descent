using ChronosDescent.Scripts.Core.State;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Temporal Echo: Attacks create delayed copies that strike again
/// </summary>
[GlobalClass]
public partial class TemporalEchoBlessing : Blessing
{
    private double _lastDamageDealt;
    private double _lastEchoTime;
    public override string Id { get; protected set; } = "temporal_echo";
    public override string Title { get; protected set; } = "Temporal Echo";

    public override string Description { get; protected set; } =
        "Your attacks have a {0}% chance to strike again after a brief delay.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Rare;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Offensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    // Configurable properties
    [Export] public double EchoChance { get; set; } = 25.0; // Default 25% chance
    [Export] public double EchoDamageMultiplier { get; set; } = 0.5; // Echo deals 50% of original damage
    [Export] public double EchoDelay { get; set; } = 1.0; // Seconds before echo strikes

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    // Format description with current echo chance
    public override void OnApply()
    {
        Description = string.Format(Description, EchoChance * CurrentLevel);
    }

    public override void OnLevelUp()
    {
        Description = string.Format(Description, EchoChance * CurrentLevel);
    }

    public override void OnDamageDealt(double amount)
    {
        // Store the damage dealt for potential echo trigger
        _lastDamageDealt = amount;

        // Roll for echo chance
        if (GD.RandRange(0, 100) < EchoChance * CurrentLevel)
            // Schedule an echo after the delay
            _lastEchoTime = EchoDelay;
    }

    public override void OnTick(double delta)
    {
        // Check if we have an echo pending
        if (_lastEchoTime > 0)
        {
            _lastEchoTime -= delta;

            // Time to trigger the echo
            if (_lastEchoTime <= 0 && Owner.GetNodeOrNull<Node2D>("TemporalEchoEffect") == null)
            {
                // Calculate echo damage
                var echoDamage = _lastDamageDealt * EchoDamageMultiplier;

                // Create visual effect
                var echoEffect = new Node2D();
                echoEffect.Name = "TemporalEchoEffect";
                Owner.AddChild(echoEffect);

                // Deal the echo damage
                // In a real implementation, we'd need to track the last hit enemy and apply the echo to them
                // For now, we'll just trigger the effect
                GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle, $"Temporal Echo: {echoDamage:F1}");

                // Clean up our effect after a short delay
                Owner.GetTree().CreateTimer(0.5).Timeout += () => echoEffect?.QueueFree();
            }
        }
    }
}

/// <summary>
///     Time Accelerated Strikes: Weapon attacks speed up with consecutive hits
/// </summary>
[GlobalClass]
public partial class TimeAcceleratedStrikesBlessing : Blessing
{
    // Runtime tracking
    private int _currentStacks;
    private double _stackTimer;
    public override string Id { get; protected set; } = "time_accelerated_strikes";
    public override string Title { get; protected set; } = "Time Accelerated Strikes";

    public override string Description { get; protected set; } =
        "Each successful hit increases your attack speed by {0}% for {1} seconds, stacking up to {2} times.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Uncommon;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Offensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aion;

    [Export] public double AttackSpeedBoostPerStack { get; set; } = 5.0; // 5% per stack
    [Export] public double StackDuration { get; set; } = 3.0; // Seconds before stacks decay
    [Export] public int MaxStacks { get; set; } = 5; // Maximum number of stacks

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnApply()
    {
        // Set up multiplicative modifier for attack speed
        MultiplicativeModifiers = new Dictionary<StatFieldSpecifier, double>();

        // Format description with current values
        Description = string.Format(Description,
            AttackSpeedBoostPerStack * CurrentLevel,
            StackDuration,
            MaxStacks);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description,
            AttackSpeedBoostPerStack * CurrentLevel,
            StackDuration,
            MaxStacks);
    }

    public override void OnDamageDealt(double amount)
    {
        // Add a stack on hit
        if (_currentStacks < MaxStacks)
        {
            _currentStacks++;

            // Reset stack timer
            _stackTimer = StackDuration;

            // Calculate boost
            var boost = 1.0 + _currentStacks * AttackSpeedBoostPerStack * CurrentLevel / 100.0;

            // Apply the attack speed boost
            MultiplicativeModifiers[StatFieldSpecifier.AttackSpeed] = boost;

            // Signal that stats need recalculation
            if (Owner?.BlessingManager != null)
                // Mark stats as dirty
                Owner.StatsManager?.Recalculate(new System.Collections.Generic.Dictionary<StatFieldSpecifier, double>(),
                    Utils.ToDictionary(MultiplicativeModifiers));
        }
    }

    public override void OnTick(double delta)
    {
        // Decay stacks over time
        if (_currentStacks > 0)
        {
            _stackTimer -= delta;

            if (_stackTimer <= 0)
            {
                // Remove all stacks
                _currentStacks = 0;

                // Reset attack speed modifier
                MultiplicativeModifiers[StatFieldSpecifier.AttackSpeed] = 1.0;

                // Signal that stats need recalculation
                if (Owner?.BlessingManager != null)
                    // Mark stats as dirty
                    Owner.StatsManager?.Recalculate(
                        new System.Collections.Generic.Dictionary<StatFieldSpecifier, double>(),
                        Utils.ToDictionary(MultiplicativeModifiers));
            }
        }
    }
}

/// <summary>
///     Entropy Buildup: Consecutive hits on same target deal increasing damage
/// </summary>
[GlobalClass]
public partial class EntropyBuildupBlessing : Blessing
{
    private int _hitCounter;

    // Runtime tracking
    private ulong _lastTargetId;
    private double _resetTimer;
    public override string Id { get; protected set; } = "entropy_buildup";
    public override string Title { get; protected set; } = "Entropy Buildup";

    public override string Description { get; protected set; } =
        "Consecutive hits on the same target deal {0}% more damage, stacking up to {1}%.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Epic;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Offensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Aevum;

    [Export] public double DamageIncreasePerHit { get; set; } = 5.0; // 5% per hit
    [Export] public double MaxDamageIncrease { get; set; } = 25.0; // Up to 25%
    [Export] public double ResetTime { get; set; } = 2.0; // Seconds before entropy resets

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 3;

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description,
            DamageIncreasePerHit * CurrentLevel,
            MaxDamageIncrease * CurrentLevel);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description,
            DamageIncreasePerHit * CurrentLevel,
            MaxDamageIncrease * CurrentLevel);
    }

    public override void OnDamageDealt(double amount)
    {
        // In a real implementation, we'd get the actual target ID
        // For now, we'll simulate with a random number
        var targetId = (ulong)GD.RandRange(1, 3);

        // Check if this is the same target
        if (_lastTargetId == targetId)
        {
            // Increment hit counter
            _hitCounter++;

            // Reset the timer
            _resetTimer = ResetTime;
        }
        else
        {
            // New target, reset counter
            _lastTargetId = targetId;
            _hitCounter = 1;
            _resetTimer = ResetTime;
        }
    }

    public override void OnTick(double delta)
    {
        // Decay the hit counter over time
        if (_hitCounter > 0)
        {
            _resetTimer -= delta;

            if (_resetTimer <= 0)
            {
                // Reset entropy
                _hitCounter = 0;
                _lastTargetId = 0;
            }
        }
    }

    // Calculate damage bonus based on hit counter
    public double GetDamageMultiplier()
    {
        // Calculate bonus percentage
        var bonus = Mathf.Min(
            DamageIncreasePerHit * CurrentLevel * _hitCounter,
            MaxDamageIncrease * CurrentLevel);

        // Convert to multiplier (e.g., 15% becomes 1.15)
        return 1.0 + bonus / 100.0;
    }
}

/// <summary>
///     Paradox Damage: Chance for hits to strike both current and "future" enemy state
/// </summary>
[GlobalClass]
public partial class ParadoxDamageBlessing : Blessing
{
    public override string Id { get; protected set; } = "paradox_damage";
    public override string Title { get; protected set; } = "Paradox Damage";

    public override string Description { get; protected set; } =
        "Attacks have a {0}% chance to deal {1}% additional damage to both current and future states of the target.";

    [Export] public override BlessingRarity Rarity { get; set; } = BlessingRarity.Legendary;
    [Export] public override BlessingCategory Category { get; set; } = BlessingCategory.Offensive;
    [Export] public override TimeDeity Deity { get; set; } = TimeDeity.Kairos;

    [Export] public double ParadoxChance { get; set; } = 15.0; // 15% chance to trigger
    [Export] public double ParadoxDamagePercent { get; set; } = 100.0; // 100% additional damage

    public override bool IsStackable { get; protected set; } = true;
    public override int MaxLevel { get; protected set; } = 2;

    public override void OnApply()
    {
        // Format description with current values
        Description = string.Format(Description,
            ParadoxChance * CurrentLevel,
            ParadoxDamagePercent);
    }

    public override void OnLevelUp()
    {
        // Update description with new values
        Description = string.Format(Description,
            ParadoxChance * CurrentLevel,
            ParadoxDamagePercent);
    }

    public override void OnDamageDealt(double amount)
    {
        // Check if paradox damage triggers
        if (GD.RandRange(0, 100) < ParadoxChance * CurrentLevel)
        {
            // Calculate paradox damage
            var paradoxDamage = amount * (ParadoxDamagePercent / 100.0);

            // Display effect
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BoardcastTitle, $"Paradox Damage: {paradoxDamage:F1}");

            // In a real implementation, we'd need to apply the damage to the target
            // and create a delayed damage effect for the "future state"

            // Instead, just create a visual effect
            var paradoxEffect = new Node2D();
            paradoxEffect.Name = "ParadoxEffect";
            Owner.AddChild(paradoxEffect);

            // Clean up our effect after a short delay
            Owner.GetTree().CreateTimer(0.5).Timeout += () => paradoxEffect?.QueueFree();
        }
    }
}