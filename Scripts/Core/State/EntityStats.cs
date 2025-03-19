using System;
using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Core.State;

public enum StatFieldSpecifier
{
    Health,
    MaxHealth,
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

public class EntityStats(EntityBaseStats baseStats)
{
    public double AttackSpeed = baseStats.AttackSpeed;
    public double CriticalChance = baseStats.CriticalChance;
    public double CriticalDamage = baseStats.CriticalDamage;
    public double Defense = baseStats.Defense;
    public double Health = baseStats.Health;
    public double MaxHealth = baseStats.Health;
    public double MoveSpeed = baseStats.MoveSpeed;
}

public class Manager : ISystem
{
    private readonly EntityBaseStats _baseStats;

    public Manager(EntityBaseStats baseStats)
    {
        _baseStats = baseStats;
        CurrentStats = new EntityStats(_baseStats);
    }

    public EntityStats CurrentStats { get; private set; }

    public double AttackSpeed => CurrentStats.AttackSpeed;
    public double CriticalChance => CurrentStats.CriticalChance;
    public double CriticalDamage => CurrentStats.CriticalDamage;
    public double Defense => CurrentStats.Defense;

    public double Health
    {
        get => CurrentStats.Health;
        set => CurrentStats.Health = Math.Min(value, _baseStats.Health);
    }

    public double MaxHealth => _baseStats.Health;
    public double MoveSpeed => CurrentStats.MoveSpeed;


    public void Initialize(IEntity owner)
    {
    }

    public void Update(double deltaTime)
    {
    }

    public void FixedUpdate(double deltaTime)
    {
    }


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
        foreach (var (stat, value) in multiplicativeTotal)
            switch (stat)
            {
                case StatFieldSpecifier.Health:
                    CurrentStats.Health *= value;
                    break;
                case StatFieldSpecifier.MaxHealth:
                    CurrentStats.MaxHealth *= value;
                    break;
                case StatFieldSpecifier.Defense:
                    CurrentStats.Defense *= value;
                    break;
                case StatFieldSpecifier.CriticalChance:
                    CurrentStats.CriticalChance *= value;
                    break;
                case StatFieldSpecifier.CriticalDamage:
                    CurrentStats.CriticalDamage *= value;
                    break;
                case StatFieldSpecifier.MoveSpeed:
                    CurrentStats.MoveSpeed *= value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        
        
        foreach (var (stat, value) in additiveTotal)
            switch (stat)
            {
                case StatFieldSpecifier.Health:
                    CurrentStats.Health += value;
                    break;
                case StatFieldSpecifier.MaxHealth:
                    CurrentStats.MaxHealth += value;
                    break;
                case StatFieldSpecifier.Defense:
                    CurrentStats.Defense += value;
                    break;
                case StatFieldSpecifier.CriticalChance:
                    CurrentStats.CriticalChance += value;
                    break;
                case StatFieldSpecifier.CriticalDamage:
                    CurrentStats.CriticalDamage += value;
                    break;
                case StatFieldSpecifier.MoveSpeed:
                    CurrentStats.MoveSpeed += value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }
}