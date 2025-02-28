namespace ChronosDescent.Scripts.resource;

using Godot;

public partial class BaseStats : Resource
{
    // Basic stats with private backing fields and public properties
    [Export] public double Health = 100;
    [Export] public double MaxHealth = 100;
    [Export] public double Mana = 100;
    [Export] public double MaxMana = 100;
    [Export] public double Defense = 100;
    [Export] public double Strength = 10;
    [Export] public double Intelligence = 10;
    [Export] public double CriticalChance = 50;
    [Export] public double CriticalDamage = 100;
    [Export] public double AttackSpeed = 4;
    [Export] public double MoveSpeed = 300;
}