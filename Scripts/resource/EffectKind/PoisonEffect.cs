using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource.EffectKind;

[GlobalClass]
public partial class PoisonEffect : Effect
{
    public PoisonEffect()
    {
        Name = "Poison";
        Description = "Takes damage over time";
        Type = EffectType.OverTime;
        Duration = 10.0;
        TickInterval = 1.0;
        IsStackable = true;
        MaxStacks = 5;
    }


    public override void OnTick(Entity target)
    {
        target.TakeDamage(10);
    }
}