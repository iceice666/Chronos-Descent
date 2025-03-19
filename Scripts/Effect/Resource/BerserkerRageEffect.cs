using Godot;
using Godot.Collections;
using BaseStats = ChronosDescent.Scripts.Entity.Resource.BaseStats;

namespace ChronosDescent.Scripts.Effect.Resource;

[GlobalClass]
public sealed partial class BerserkerRageEffect : Effect
{
    private double _lastCalculatedBonus;

    public BerserkerRageEffect()
    {
        Identifier = "berserker_rage";
        Name = "Berserker Rage Effect";
        Description = "Increases damage as health decreases below threshold";
        Behaviors = EffectBehavior.StatModifier | EffectBehavior.PeriodicTick;
        Duration = 20.0;
        TickInterval = 0.5;

        AdditiveModifiers = new Dictionary<BaseStats.Specifier, double>
        {
            { BaseStats.Specifier.CriticalChance, 10 },
            { BaseStats.Specifier.CriticalDamage, 0 }
        };
    }

    [Export] public double HealthThreshold { get; set; } = 0.3; // 30% health
    [Export] public double MaxStrengthBonus { get; set; } = 50.0; // Max strength bonus


    public override void OnApply()
    {
        // Set initial values
        UpdateStrengthBonus();
        GD.Print($"Berserker Rage applied to {Target.Name}");
    }

    public override void OnTick(double delta)
    {
        UpdateStrengthBonus();
    }

    private void UpdateStrengthBonus()
    {
        // Calculate health percentage
        var healthPercentage = Target.Stats.Health / Target.Stats.MaxHealth;

        // Only apply effects below threshold
        if (healthPercentage <= HealthThreshold)
        {
            // Calculate dynamic strength bonus (more bonus as health gets lower)
            var normalizedHealthLoss = (HealthThreshold - healthPercentage) / HealthThreshold;
            var strengthBonus = MaxStrengthBonus * normalizedHealthLoss;

            // Only update the modifier if it changed significantly (reduce unnecessary updates)
            if (Mathf.Abs(_lastCalculatedBonus - strengthBonus) > 1.0)
            {
                _lastCalculatedBonus = strengthBonus;
                AdditiveModifiers[BaseStats.Specifier.CriticalDamage] = strengthBonus;

                // Mark stats for recalculation
                Target.EffectManager.SetStatsDirty();

                GD.Print(
                    $"Berserker Rage strength bonus updated: {strengthBonus:F1}% at {healthPercentage * 100:F1}% health");
            }
        }
        else if (_lastCalculatedBonus > 0)
        {
            // Reset bonus if health is above threshold
            _lastCalculatedBonus = 0;
            AdditiveModifiers[BaseStats.Specifier.CriticalDamage] = 0;

            // Mark stats for recalculation
            Target.EffectManager.SetStatsDirty();
        }
    }

    public override void OnRemove()
    {
        GD.Print($"Berserker Rage removed from {Target.Name}");
    }
}
