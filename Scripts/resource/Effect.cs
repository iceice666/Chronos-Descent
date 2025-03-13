using System;
using ChronosDescent.Scripts.node;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.resource;

[GlobalClass]
public partial class Effect : Resource
{
    [Flags]
    public enum EffectBehavior
    {
        StatModifier = 1, // Modifies stats directly
        ControlEffect = 2, // CC effects (stun, root, etc.)
        PeriodicTick = 4, // Needs regular updates
        SpecialLogic = 8 // Custom game logic
    }

    public string Identifier = "unknown";
    public string Name { get; protected set; } = "Unknown Effect";
    public string Description { get; protected set; } = "Description";
    public Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/Effect/MissingIcon.png");
    public EffectBehavior Behaviors { get; protected set; }
    public double TickInterval { get; protected set; }
    public double Duration { get; set; }
    public bool IsPermanent { get; protected set; }
    public bool IsStackable { get; protected set; }
    public int MaxStacks { get; protected set; } = 1;

    // Modifiers dictionary - efficient lookup by stat type
    public Dictionary<BaseStats.Specifier, double> AdditiveModifiers { get; protected set; } = [];
    public Dictionary<BaseStats.Specifier, double> MultiplicativeModifiers { get; protected set; } = [];

    public Entity Target { get; set; }

    // Helper properties
    public bool HasStatModifiers =>
        Behaviors.HasFlag(EffectBehavior.StatModifier) &&
        (AdditiveModifiers.Count > 0 || MultiplicativeModifiers.Count > 0);

    public bool NeedsTicking => Behaviors.HasFlag(EffectBehavior.PeriodicTick);

    public bool IsControlEffect => Behaviors.HasFlag(EffectBehavior.ControlEffect);

    //  methods that derived classes must implement
    public virtual void OnApply()
    {
    }

    public virtual void OnRemove()
    {
    }

    // Virtual methods that derived classes can optionally override
    public virtual void OnTick(double delta)
    {
    }

    public virtual void OnTick(double delta, int currentStacks)
    {
    }

    public virtual void OnStack(int currentStacks)
    {
    }
}