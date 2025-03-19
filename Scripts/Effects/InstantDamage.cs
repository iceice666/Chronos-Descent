using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using Godot;

namespace ChronosDescent.Scripts.Effects;

[GlobalClass]
public sealed partial class InstantDamage : BaseEffect
{
    [Export] public double Damage { get; set; }
    
    public InstantDamage()
    {
        Id = "instant_damage";
        Description = "Deal damage";
        Duration = 1e-6;
    }

    public override void OnApply()
    {
        Owner.TakeDamage(Damage, DamageType.Normal);
    }
}