using System;
using Godot;

namespace ChronosDescent.Scripts.resource;

public partial class BaseStats : Resource, ICloneable
{
    public enum CombatResource
    {
        Mana,
        Ammo,
        Energy,
    }
    
    // Basic stats with private backing fields and public properties
    [Export] public double Health = 100;
    [Export] public double MaxHealth = 100;
    [Export] public CombatResource ResourceType { get; set; } = CombatResource.Energy;
    [Export] public double CurrentResource { get; set; } = 100;
    [Export] public double MaxResource { get; set; } = 100;
    [Export] public double Defense = 100;
    [Export] public double CriticalChance = 50;
    [Export] public double CriticalDamage = 100;
    [Export] public double AttackSpeed = 4;
    [Export] public double MoveSpeed = 300;

    public enum Specifier
    {
        Health,
        MaxHealth,
        CurrentResource,
        MaxResource,
        Defense,
        CriticalChance,
        CriticalDamage,
        AttackSpeed,
        MoveSpeed
    }

    // Implement ICloneable interface
    public object Clone()
    {
        return new BaseStats
        {
            Health = this.Health,
            MaxHealth = this.MaxHealth,
            ResourceType = this.ResourceType,
            CurrentResource = this.CurrentResource,
            MaxResource = this.MaxResource,
            Defense = this.Defense,
            CriticalChance = this.CriticalChance,
            CriticalDamage = this.CriticalDamage,
            AttackSpeed = this.AttackSpeed,
            MoveSpeed = this.MoveSpeed
        };
    }
    
    // Convenience method to create typed clone
    public BaseStats DeepCopy()
    {
        return (BaseStats)Clone();
    }
}