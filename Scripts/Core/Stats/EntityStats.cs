using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Core.Stats;

public enum StatFieldSpecifier
{
    Health,
    MaxHealth,
    CurrentResource,
    MaxResource,
    Defense,
    CriticalChance,
    CriticalDamage,
    MoveSpeed
}

public class EntityBaseStats
{
    public double Health { get; } = 100;
    public double Defense { get; } = 10;
    public double CriticalChance { get; } = 5;
    public double CriticalDamage { get; } = 50;
    public double MoveSpeed { get; } = 100;
    public double AttackSpeed { get; } = 1;
}

public class EntityStats
{
    public double AttackSpeed;
    public double CriticalChance;
    public double CriticalDamage;
    public double Defense;
    public double Health;
    public double MaxHealth;
    public double MoveSpeed;

    public EntityStats(EntityBaseStats baseStats)
    {
        Health = baseStats.Health;
        MaxHealth = baseStats.Health;
        Defense = baseStats.Defense;
        CriticalChance = baseStats.CriticalChance;
        CriticalDamage = baseStats.CriticalDamage;
        AttackSpeed = baseStats.AttackSpeed;
        MoveSpeed = baseStats.MoveSpeed;
    }
}

public class Manager : IManager
{
    public Manager(EntityBaseStats baseStats)
    {
        _baseStats = baseStats;
        CurrentStats = new EntityStats(_baseStats);
    }

    private readonly EntityBaseStats _baseStats;
    public EntityStats CurrentStats { get; private set; }


    public void FullReset()
    {
        CurrentStats = new EntityStats(_baseStats);
    }

    public void Reset()
    {
        var originHealth = CurrentStats.Health;

        CurrentStats = new EntityStats(_baseStats)
        {
            Health = originHealth
        };
    }

    public void Recalculate(
        Dictionary<StatFieldSpecifier, double> additiveTotal,
        Dictionary<StatFieldSpecifier, double> multiplicativeTotal)
    {
       
    }

    public void Initialize(IEntity owner)
    {
    }

    public void Update(double deltaTime)
    {
    }

    public void FixedUpdate(double deltaTime)
    {
    }
}