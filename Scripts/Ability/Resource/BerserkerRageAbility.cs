using Godot;
using BerserkerRageEffect = ChronosDescent.Scripts.Effect.Resource.BerserkerRageEffect;

namespace ChronosDescent.Scripts.Ability.Resource;

[GlobalClass]
public partial class BerserkerRageAbility : BasePassiveAbility
{
    private BerserkerRageEffect _rageEffect;

    public BerserkerRageAbility()
    {
        Name = "Berserker Rage";
        Description = "Passively gain increased strength as health decreases below 30%";
        Cooldown = 0.0; // Passive abilities don't have cooldowns

        _rageEffect = new BerserkerRageEffect();
    }

    [Export] public double HealthThreshold { get; set; } = 0.3; // 30% health
    [Export] public double MaxStrengthBonus { get; set; } = 50.0; // Maximum strength bonus


    protected override void OnPassiveTick(double delta)
    {
        // Check if health is below the threshold
        var healthPercentage = Caster.Stats.Health / Caster.Stats.MaxHealth;

        if (healthPercentage <= HealthThreshold)
        {
            // Apply rage effect if not already applied
            if (Caster.HasEffect(_rageEffect.Name)) return;
            Caster.ApplyEffect(_rageEffect);
        }
        else
        {
            // Remove rage effect if health is above the threshold
            if (!Caster.HasEffect(_rageEffect.Name)) return;
            Caster.RemoveEffect(_rageEffect.Name);
        }
    }
}
