using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource.EffectKind;

[GlobalClass]
public partial class SpeedBoostEffect : Effect
{
    public SpeedBoostEffect()
    {
        Name = "Speed Boost";
        Description = "Increases movement speed";
        Type = EffectType.Buff;
        Duration = 5.0;
        MoveSpeedModifier = 100; 
        IsStackable = true;
        MaxStacks = 5;
    }

    public override void OnApply(Entity target)
    {
        // Could play a speed effect particle
        GD.Print($"Speed boost applied to {target.Name}");
    }
}