using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class StatsComponent : Node
{
    // Events
    [Signal]
    public delegate void StatsChangedEventHandler();

    private const double MaxMoveSpeed = 1000;
    public BaseStats Current;
    [Export] public BaseStats Base { get; private set; } = new();


    // Getter methods that expose CurrentStats properties
    public double Health
    {
        get => Current.Health;
        set => Current.Health = value;
    }

    public double MaxHealth
    {
        get => Current.MaxHealth;
        set => Current.MaxHealth = value;
    }

    public double Defense
    {
        get => Current.Defense;
        set => Current.Defense = value;
    }

    public BaseStats.CombatResource ResourceType
    {
        get => Current.ResourceType;
        set => Current.ResourceType = value;
    }

    public double CurrentResource
    {
        get => Current.CurrentResource;
        set => Current.CurrentResource = value;
    }

    public double MaxResource
    {
        get => Current.MaxResource;
        set => Current.MaxResource = value;
    }

    public double CriticalChance
    {
        get => Current.CriticalChance;
        set => Current.CriticalChance = value;
    }

    public double CriticalDamage
    {
        get => Current.CriticalDamage;
        set => Current.CriticalDamage = value;
    }

    public double AttackSpeed
    {
        get => Current.AttackSpeed;
        set => Current.AttackSpeed = value;
    }

    public double MoveSpeed => Mathf.Clamp(Current.MoveSpeed, 0, MaxMoveSpeed);


    public override void _Ready()
    {
        ResetStatsToBase();
    }

    public void ResetStatsToBase()
    {
        Current = (BaseStats)Base.Clone();

        EmitSignal(SignalName.StatsChanged);
    }
}