using System;
using Godot;

namespace ChronosDescent.Scripts.Entity.Resource;

[GlobalClass]
public partial class BaseStats : Godot.Resource, ICloneable
{
    public enum CombatResource
    {
        Mana,
        Ammo,
        Energy
    }

    public enum Specifier
    {
        Health,
        MaxHealth,
        CurrentResource,
        MaxResource,
        Defense,
        CriticalChance,
        CriticalDamage,
        AttackSpeed, // attack (AttackSpeed)/100 per second
        MoveSpeed
    }

    [Export] public double AttackSpeed = 100;
    [Export] public double CriticalChance = 5;
    [Export] public double CriticalDamage = 50;
    [Export] public double Defense = 100;

    // Basic stats with private backing fields and public properties
    [Export] public double Health = 100;
    [Export] public double MaxHealth = 100;
    [Export] public double MoveSpeed = 300;
    [Export] public CombatResource ResourceType { get; set; } = CombatResource.Energy;
    [Export] public double CurrentResource { get; set; } = 100;
    [Export] public double MaxResource { get; set; } = 100;

    // Implement ICloneable interface
    public object Clone()
    {
        return new BaseStats
        {
            Health = Health,
            MaxHealth = MaxHealth,
            ResourceType = ResourceType,
            CurrentResource = CurrentResource,
            MaxResource = MaxResource,
            Defense = Defense,
            CriticalChance = CriticalChance,
            CriticalDamage = CriticalDamage,
            AttackSpeed = AttackSpeed,
            MoveSpeed = MoveSpeed
        };
    }

    // Convenience method to create typed clone
    public BaseStats DeepCopy()
    {
        return (BaseStats)Clone();
    }
}
