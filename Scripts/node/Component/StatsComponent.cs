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
    public double Health => _currentStats.Health;
    public double MaxHealth => _currentStats.MaxHealth;
    public double Mana => _currentStats.Mana;
    public double MaxMana => _currentStats.MaxMana;
    public double Defense => _currentStats.Defense;
    public double Strength => _currentStats.Strength;
    public double Intelligence => _currentStats.Intelligence;
    public double CriticalChance=> _currentStats.CriticalChance;
    public double CriticalDamage => _currentStats.CriticalDamage;
    public double AttackSpeed => _currentStats.AttackSpeed;

    public double MoveSpeed => _currentStats.MoveSpeed <= MaxMoveSpeed
        ? _currentStats.MoveSpeed
        : MaxMoveSpeed;

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