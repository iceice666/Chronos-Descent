using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using Godot;

namespace ChronosDescent.Scripts.Effects;

[GlobalClass]
public sealed partial class InstantDamage : BaseEffect
{
    [Export] public double Damage { get; set; }

    public override string Id { get; protected set; } = "instant_damage";
    public override double Duration { get; set; } = 1e-6;
    public override string Description { get; protected set; } = "Deal damage";

    public override void OnApply()
    {
        Owner.TakeDamage(Damage, DamageType.Normal);
    }
}