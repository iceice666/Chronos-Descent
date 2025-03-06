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
    public double Defense => _currentStats.Defense;
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


}