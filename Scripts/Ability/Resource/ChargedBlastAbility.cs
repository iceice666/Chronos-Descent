using ChronosDescent.Scripts.Ability.Node;
using ChronosDescent.Scripts.Ability.UI;
using Godot;

namespace ChronosDescent.Scripts.Ability.Resource;

[GlobalClass]
public partial class ChargedBlastAbility : BaseChargedAbility
{
    public ChargedBlastAbility()
    {
        Name = "Charged Blast";
        Description = "Hold to charge a powerful blast. Longer charge increases radius and damage.";
        Cooldown = 8.0;
        MaxChargeTime = 2.0;
        MinChargeTime = 0.3;
        IndicatorConfig = new IndicatorConfig(DrawMode.Circle, MinRadius, MaxRadius);
        RequiredSlot = AbilitySlot.WeaponSpecial;
    }

    [Export] public double BaseDamage { get; set; } = 30.0;

    [Export] public double MinRadius { get; set; } = 50.0;

    [Export] public double MaxRadius { get; set; } = 200.0;


    protected override void ExecuteEffect(double powerMultiplier)
    {
        // Calculate damage and radius based on charge level
        var damage = BaseDamage * (0.5 + 0.5 * powerMultiplier);
        var range = MinRadius + (MaxRadius - MinRadius) * powerMultiplier;

        // Find all entities in the blast radius
        var targets = Caster.GetTree().GetNodesInGroup("Entity");

        var origin = Caster.GlobalPosition;

        foreach (var node in targets)
            if (node is Entity.Entity target && target != Caster)
            {
                var distance = origin.DistanceTo(target.GlobalPosition);

                if (distance > range)
                    continue;

                // Apply damage
                target.TakeDamage(damage);

                GD.Print($"{Name} hit {target.Name} for {damage} damage");
            }
    }

    protected override void OnChargingCanceled()
    {
        if (!IsCharging)
            return;

        IsCharging = false;
        CurrentChargeTime = 0.0;

        GD.Print($"{Name} charge canceled");
    }
}