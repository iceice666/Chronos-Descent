// Scripts/resource/Effects/Example/StunEffect.cs

using ChronosDescent.Scripts.node;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.resource.Effects.Example;

[GlobalClass]
public sealed partial class StunEffect : Effect
{
   public StunEffect()
    {
        Identifier = "stun";
        Name = "Stun";
        Duration = 2.0;
        Description = "Prevents movement and actions";
        Behaviors = EffectBehavior.ControlEffect;
    }


    public override void OnApply()
    {
        // Disable movement
        Target.Moveable = false;

        GD.Print($"{Target.Name} is stunned");
    }

    public override void OnRemove()
    {
        // Re-enable movement
        Target.Moveable = true;

        // Reset animation
        Target.Animation?.ResetAnimation();

        GD.Print($"{Target.Name} is no longer stunned");
    }
}