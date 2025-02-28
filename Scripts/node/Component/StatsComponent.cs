using System;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class StatsComponent : Node
{
    [Export] private BaseStats _baseStats = new();
    private BaseStats _currentStats = new();

    // Getter methods that expose CurrentStats properties
    public double GetHealth() => _currentStats.Health;
    public double GetMaxHealth() => _currentStats.MaxHealth;
    public double GetMana() => _currentStats.Mana;
    public double GetMaxMana() => _currentStats.MaxMana;
    public double GetDefense() => _currentStats.Defense;
    public double GetStrength() => _currentStats.Strength;
    public double GetIntelligence() => _currentStats.Intelligence;
    public double GetCriticalChance() => _currentStats.CriticalChance;
    public double GetCriticalDamage() => _currentStats.CriticalDamage;
    public double GetAttackSpeed() => _currentStats.AttackSpeed;
    public double GetMoveSpeed() => _currentStats.MoveSpeed;

    private const double MaxMoveSpeed = 1000;

    // Events
    [Signal]
    public delegate void StatsChangedEventHandler();


    public override void _Ready()
    {
        ResetStatsToBase();
    }

    public void ResetStatsToBase()
    {
        _currentStats = _baseStats;

        EmitSignal(SignalName.StatsChanged);
    }

    public void UpdateStats(Action<BaseStats> cb)
    {
        cb(_currentStats);
        EmitSignal(SignalName.StatsChanged);
    }

    public void PrintStats()
    {
        GD.Print($"Health: {_currentStats.Health}/{_currentStats.MaxHealth}");
        GD.Print($"Mana: {_currentStats.Mana}/{_currentStats.MaxMana}");
        GD.Print($"Defense: {_currentStats.Defense}");
        GD.Print($"Strength: {_currentStats.Strength}");
        GD.Print($"Intelligence: {_currentStats.Intelligence}");
        GD.Print($"Critical Chance: {_currentStats.CriticalChance}%");
        GD.Print($"Critical Damage: {_currentStats.CriticalDamage}%");
        GD.Print($"Attack Speed: {_currentStats.AttackSpeed} times/sec");
        GD.Print($"Move Speed: {_currentStats.MoveSpeed} units/sec");
    }
}