// Scripts/resource/Effects/Example/PoisonEffect.cs

using Godot;
using Godot.Collections;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.UI;

namespace ChronosDescent.Scripts.resource.Effects.Example;

[GlobalClass]
public sealed partial class PoisonEffect : Effect
{
    public PoisonEffect()
    {
        Identifier = "poison";
        Name = "Poison Effect";
        Description = "Takes damage over time and reduces defense";
        Duration = 10.0;
        TickInterval = 1.0;
        Behaviors = EffectBehavior.PeriodicTick | EffectBehavior.StatModifier;
        IsStackable = true;
        MaxStacks = 5;

        AdditiveModifiers =
            new Dictionary<BaseStats.Specifier, double>
            {
                {
                    BaseStats.Specifier.Defense, -DefenseReduction
                }
            };
    }

    [Export] public double DamagePerTick { get; set; } = 10.0;
    [Export] public double DefenseReduction { get; set; } = 5.0;


    public override void OnApply()
    {
        GD.Print($"Poison applied to {Target.Name}");
    }

    public override void OnTick(double delta, int stacks)
    {
        var damage = DamagePerTick * stacks;

        Target.TakeDamage(damage, DamageIndicator.DamageType.Poison);

        GD.Print($"{Target.Name} takes {damage} poison damage ({stacks} stacks)");
    }

    public override void OnStack(int currentStacks)
    {
        // Recalculate defense reduction based on stacks
        AdditiveModifiers[BaseStats.Specifier.Defense] = -DefenseReduction * currentStacks;

        GD.Print($"Poison stacked to {currentStacks} on {Target.Name}");
    }
}