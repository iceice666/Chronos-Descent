using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource.EffectKind;
[GlobalClass]
public partial class BerserkerRageEffect : Effect
{
    private double _healthThreshold = 0.3; // 30% health

    public BerserkerRageEffect()
    {
        Name = "Berserker Rage";
        Description = "Increases damage as health decreases";
        Type = EffectType.Special;
        Duration = 20.0;
        TickInterval = 0.5; // Check frequently
    }

    public override void OnTick(Entity target)
    {
        // Calculate buff based on missing health percentage
        var healthPercentage = target.Stats.Health / target.Stats.MaxHealth;

        if (healthPercentage > _healthThreshold) return;
        
        // Dynamically adjust strength based on health percentage
        // The lower the health, the higher the strength bonus
        var strengthBonus = 20 * ((_healthThreshold - healthPercentage) / _healthThreshold);
            
        // We're using OnTick to dynamically update the strength modifier
        StrengthModifier = strengthBonus;
            
        // Force recalculation of stats (normally happens when effects are applied/removed)
        target.UpdateStats();
        GD.Print($"Berserker strength bonus: {strengthBonus}");
    }
}