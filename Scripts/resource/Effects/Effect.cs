using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource;



[GlobalClass]
// ReSharper disable once RedundantNameQualifier
public partial class Effect : Godot.Resource
{
    [ExportGroup("Metadata")] [Export] public string Name { get; set; } = "Effect";
    [Export] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }
    [Export] public double TickInterval { get; set; } = 1.0; // How often the effect ticks
    [Export] public double Duration { get; set; } = 5.0;
    [Export] public bool IsPermanent { get; set; }
    [Export] public bool IsStackable { get; set; }
    [Export] public int MaxStacks { get; set; } = 1;


    
    [GlobalClass]
    public partial class EffectModifier : Resource
    {
        [Export] private Effect.ModifierType _type;
        [Export] private BaseStats.Specifier _specifier;
    }

    [ExportGroup("Modifiers")]
    // Effect properties for various stats
    [Export]
    public EffectModifier[] Modifiers { get; set; } = [];


    public enum ModifierType
    {
        Additive,
        Multiplicative,
    }


    // Custom effect type for special effects
    public enum EffectType
    {
        Buff,
        Debuff,
        OverTime,
        Special
    }

    [Export] public EffectType Type { get; set; } = EffectType.Buff;

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

