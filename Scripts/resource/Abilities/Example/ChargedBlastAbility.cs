using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource.Abilities.Example;

[GlobalClass]
public partial class ChargedBlastAbility : Ability
{
    [Export] public double BaseDamage { get; set; } = 30.0;
    [Export] public double MinRadius { get; set; } = 50.0;
    [Export] public double MaxRadius { get; set; } = 200.0;
    [Export] public double Range { get; set; } = 300.0;

    public ChargedBlastAbility()
    {
        Name = "Charged Blast";
        Description = "Hold to charge a powerful blast. Longer charge increases radius and damage.";
        Type = AbilityType.Charged;
        Cooldown = 8.0;
        MaxChargeTime = 2.0;
        MinChargeTime = 0.3;
    }

    public override void Activate()
    {
        if (!CanActivate()) return;

        // Start charging
        IsCharging = true;
        CurrentChargeTime = 0.0;

        GD.Print($"{Caster.Name} started charging {Name}");
    }

    public override void ReleaseCharge()
    {
        if (!IsCharging) return;

        // Calculate charge percentage
        var chargePercentage = (CurrentChargeTime - MinChargeTime) / (MaxChargeTime - MinChargeTime);
        chargePercentage = Mathf.Clamp(chargePercentage, 0.0, 1.0);

        // Execute with appropriate power
        ExecuteEffect(chargePercentage);

        // Reset charging state
        IsCharging = false;
        CurrentChargeTime = 0.0;

        // Start cooldown
        StartCooldown();

        GD.Print($"{Caster.Name} released {Name} with {chargePercentage * 100}% charge");
    }

    protected override void ExecuteEffect(double powerMultiplier)
    {
        // Calculate damage and radius based on charge level
        var damage = BaseDamage * (0.5 + 0.5 * powerMultiplier);
        var radius = MinRadius + (MaxRadius - MinRadius) * powerMultiplier;

        // Get aim direction
        Vector2 direction = Caster.AimDirection;

        // Calculate target position
        var targetPosition = Caster.GlobalPosition + direction.Normalized() * (float)Range;

        // Find all entities in the blast radius
        var targets = Caster.GetTree().GetNodesInGroup("Entity");

        foreach (var node in targets)
        {
            if (node is Entity target && target != Caster)
            {
                var distance = targetPosition.DistanceTo(target.GlobalPosition);

                if (distance <= radius)
                {
                    // Calculate damage falloff based on distance from center
                    var distanceFactor = 1.0 - distance / radius;
                    var finalDamage = damage * distanceFactor;

                    // Apply damage
                    target.TakeDamage(finalDamage);

                    GD.Print($"{Name} hit {target.Name} for {finalDamage} damage");
                }
            }
        }
    }

    public override void CancelCharge()
    {
        if (!IsCharging) return;


        IsCharging = false;
        CurrentChargeTime = 0.0;

        GD.Print($"{Name} charge canceled");
    }
}