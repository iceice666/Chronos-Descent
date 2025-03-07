// Scripts/resource/Effects/Example/SpeedBoostEffect.cs

using ChronosDescent.Scripts.node;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.resource.Effects.Example;

[GlobalClass]
public sealed partial class SpeedBoostEffect : Effect
{
    public SpeedBoostEffect()
    {
        Identifier = "speed";
        Name = "Speed Boost Effect";
        Description = "Increases movement speed";
        Behaviors = EffectBehavior.StatModifier;
        Duration = 5.0;
        IsStackable = true;
        MaxStacks = 3;

        MultiplicativeModifiers = new Dictionary<BaseStats.Specifier, double>
        {
            { BaseStats.Specifier.MoveSpeed, SpeedMultiplier },
        };
    }


    [Export] public double SpeedMultiplier { get; set; } = 1.2;


    public override void OnApply()
    {
        // Play speed effect particle or animation if available
        GD.Print($"Speed boost applied to {Target.Name}");
    }

    public override void OnStack(int currentStacks)
    {
        GD.Print($"Speed boost stacked to level {currentStacks} on {Target.Name}");
    }

    public override void OnRemove()
    {
        GD.Print($"Speed boost removed from {Target.Name}");
    }
}