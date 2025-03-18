using ChronosDescent.Scripts.Entity.Resource;
using Godot;

namespace ChronosDescent.Scripts.Entity.Node;

[GlobalClass]
public partial class StatsComponent : Godot.Node
{
    // Events
    public delegate void StatsChangedEventHandler();

    private const double MaxMoveSpeed = 1000;
    public BaseStats Current;

    [Export] public BaseStats Base { get; private set; } = new();

    // Getter methods that expose CurrentStats properties
    public double Health
    {
        get => Current.Health;
        set
        {
            if (value <= 0)
            {
                EntityDead?.Invoke();
                Current.Health = 0;
            }
            else
            {
                Current.Health = value;
            }

            HealthStatsChanged?.Invoke();
        }
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

    public double MoveSpeed
    {
        get => Current.MoveSpeed;
        set => Current.MoveSpeed = Mathf.Clamp(value, 0, MaxMoveSpeed);
    }

    public event StatsChangedEventHandler EntityDead;
    public event StatsChangedEventHandler HealthStatsChanged;

    protected virtual void OnEntityDead()
    {
        EntityDead?.Invoke();
    }

    protected virtual void OnHealthStatsChanged()
    {
        HealthStatsChanged?.Invoke();
    }

    public override void _Ready()
    {
        Current = (BaseStats)Base.Clone();
    }

    public void ResetStatsToBase()
    {
        var newValue = (BaseStats)Base.Clone();
        newValue.Health = Current.Health;
        newValue.CurrentResource = CurrentResource;

        Current = newValue;
    }
}