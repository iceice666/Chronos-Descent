using Godot;
using BerserkerRageEffect = ChronosDescent.Scripts.resource.Effects.Example.BerserkerRageEffect;

namespace ChronosDescent.Scripts.resource.Abilities.Example;

[GlobalClass]
public partial class BerserkerRageAbility : Ability
{
    [Export] public double HealthThreshold { get; set; } = 0.3; // 30% health
    [Export] public double MaxStrengthBonus { get; set; } = 50.0; // Maximum strength bonus

    private BerserkerRageEffect _rageEffect;

    public BerserkerRageAbility()
    {
        Name = "Berserker Rage";
        Description = "Passively gain increased strength as health decreases below 30%";
        Type = AbilityType.Passive;
        Cooldown = 0.0; // Passive abilities don't have cooldowns

        _rageEffect = new BerserkerRageEffect();
    }

    protected override void OnPassiveTick(double delta)
    {
        // Check if health is below threshold
        var healthPercentage = Caster.Stats.Health / Caster.Stats.MaxHealth;

        if (healthPercentage <= HealthThreshold)
        {
            // Apply rage effect if not already applied
            if (Caster.HasEffect(_rageEffect.Name)) return;
            Caster.ApplyEffect(_rageEffect);
            GD.Print($"{Caster.Name}'s {Name} activated at {healthPercentage * 100}% health");
        }
        else
        {
            // Remove rage effect if health is above threshold
            if (!Caster.HasEffect(_rageEffect.Name)) return;
            Caster.RemoveEffect(_rageEffect.Name);
            GD.Print($"{Caster.Name}'s {Name} deactivated");
        }
    }
}