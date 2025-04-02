using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;

namespace ChronosDescent.Scripts.Abilities;

public class Heal : BaseActiveAbility
{
    private readonly double _healAmount = 30.0; // Amount to heal
    
    public Heal()
    {
        Description = "Instantly heal a significant amount of health";
    }

    public override string Id { get; protected set; } = "instant_heal";
    public override double Cooldown { get; protected init; } = 20.0;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        if (Caster == null) return;
        
        // Apply healing
        Caster.TakeDamage(_healAmount, DamageType.Healing);
        
        // Create healing visual effect
        CreateHealEffect();
    }
    
    private void CreateHealEffect()
    {
        // Here we could add visual effects for healing
        // For now, we rely on the damage indicator system which should show the healing amount
    }
}