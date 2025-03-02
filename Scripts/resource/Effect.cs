using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource;

[GlobalClass]
// ReSharper disable once RedundantNameQualifier
public partial class Effect : Godot.Resource
{
    [Export] public string Name { get; set; } = "Effect";
    [Export] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }
    [Export] public double Duration { get; set; } = 5.0; // In seconds, -1 for infinite
    [Export] public bool IsStackable { get; set; }
    [Export] public int MaxStacks { get; set; } = 1;
    [Export] public bool IsPermanent { get; set; }
    // Effect properties for various stats
    [Export] public double HealthModifier { get; set; }
    [Export] public double ManaModifier { get; set; }
    [Export] public double DefenseModifier { get; set; }
    [Export] public double StrengthModifier { get; set; }
    [Export] public double IntelligenceModifier { get; set; }
    [Export] public double CriticalChanceModifier { get; set; }
    [Export] public double CriticalDamageModifier { get; set; }
    [Export] public double AttackSpeedModifier { get; set; }
    [Export] public double MoveSpeedModifier { get; set; }

    // Periodic effect properties 
    [Export] public double TickInterval { get; set; } = 1.0; // How often the effect ticks
   

    // Custom effect type for special effects
    public enum EffectType
    {
        Buff,
        Debuff,
        OverTime,
        Special
    }

    [Export] public EffectType Type { get; set; } = EffectType.Buff;

    public void DebugPrint()
    {
        GD.Print($"{Name}: {Description}");
        GD.Print($"Duration: {Duration} secs");
        GD.Print($"IsStackable: {IsStackable}");
        GD.Print($"Stacks: {MaxStacks}");
        GD.Print($"IsPermanent: {IsPermanent}");
    }

    // Optional callback for custom effect logic
    public virtual void OnApply(Entity target)
    {
    }

    public virtual void OnRemove(Entity target)
    {
    }

    public virtual void OnTick(Entity target)
    {
    }

    public virtual void OnStack(Entity target, int currentStacks)
    {
    }
}