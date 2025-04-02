using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Rarity tiers for Temporal Blessings
/// </summary>
public enum BlessingRarity
{
    Common, // White - Small bonuses (+10-15% effects)
    Uncommon, // Green - Moderate bonuses with simple effects
    Rare, // Blue - Significant bonuses with condition triggers
    Epic, // Purple - Powerful effects that change gameplay style
    Legendary // Gold - Game-changing effects with unique visuals
}

/// <summary>
///     Categories of Temporal Blessings
/// </summary>
public enum BlessingCategory
{
    Offensive, // Damage and attack related blessings
    Defensive, // Survivability and damage reduction
    Utility, // Resource gain and miscellaneous benefits
    Movement // Movement speed and mobility
}

/// <summary>
///     Time deity sources for blessings
/// </summary>
public enum TimeDeity
{
    Chronos, // Balance - Core timing and cooldown effects
    Aion, // Cycles - Periodic effects and buffs that grow over time
    Kairos, // Opportunity - Critical hits and conditional effects
    Aevum // Eternity - Permanent upgrades and stacking effects
}

/// <summary>
///     Base class for all Temporal Blessings
/// </summary>
[GlobalClass]
public abstract partial class Blessing : Resource
{
    // Core blessing properties
    public BaseEntity Owner { get; set; }
    public abstract string Id { get; protected set; }
    public virtual string Title { get; protected set; }
    public virtual string Description { get; protected set; }
    [Export] public virtual Texture2D Icon { get; set; }

    // Classification properties
    [Export] public virtual BlessingRarity Rarity { get; set; } = BlessingRarity.Common;
    [Export] public virtual BlessingCategory Category { get; set; } = BlessingCategory.Utility;
    [Export] public virtual TimeDeity Deity { get; set; } = TimeDeity.Chronos;

    // Stat modifiers
    [Export] public virtual Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; protected set; } = new();

    [Export]
    public virtual Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; protected set; } = new();

    // Whether this blessing can be stacked multiple times (upgraded)
    [Export] public virtual bool IsStackable { get; protected set; }
    [Export] public virtual int MaxLevel { get; protected set; } = 1;
    public int CurrentLevel { get; set; } = 1;

    // Visual effect path for this blessing (if any)
    [Export] public virtual string VisualEffectPath { get; protected set; } = "";

    // Helpers
    public bool HasStatModifiers => AdditiveModifiers.Count > 0 || MultiplicativeModifiers.Count > 0;

    // Lifecycle methods
    public virtual void OnApply()
    {
    }

    public virtual void OnRemove()
    {
    }

    public virtual void OnLevelUp()
    {
    }

    // Tick method for blessings that need periodic updates
    public virtual void OnTick(double delta)
    {
    }

    // Event callbacks
    public virtual void OnAbilityUsed(AbilityVariant abilityVariant)
    {
    }

    public virtual void OnDamageDealt(double amount)
    {
    }

    public virtual void OnDamageTaken(double amount)
    {
    }

    public virtual void OnEnemyKilled()
    {
    }

    public virtual void OnTimeRewound()
    {
    }

    // Get color for rarity
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            BlessingRarity.Common => new Color(1.0f, 1.0f, 1.0f), // White
            BlessingRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
            BlessingRarity.Rare => new Color(0.2f, 0.5f, 1.0f), // Blue
            BlessingRarity.Epic => new Color(0.7f, 0.3f, 0.9f), // Purple
            BlessingRarity.Legendary => new Color(1.0f, 0.84f, 0.0f), // Gold
            _ => new Color(1.0f, 1.0f, 1.0f)
        };
    }

    // Get text description for deity
    public static string GetDeityName(TimeDeity deity)
    {
        return deity switch
        {
            TimeDeity.Chronos => "Chronos, Keeper of Balance",
            TimeDeity.Aion => "Aion, Master of Cycles",
            TimeDeity.Kairos => "Kairos, Lord of Opportunity",
            TimeDeity.Aevum => "Aevum, Guardian of Eternity",
            _ => "Unknown Deity"
        };
    }
}

/// <summary>
///     Ability variant used for blessing callbacks
/// </summary>
public enum AbilityVariant
{
    Normal,
    Special,
    Ultimate,
    TimeRewind,
    Dash,
    Other
}