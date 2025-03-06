using Godot;
using ChronosDescent.Scripts.node;

namespace ChronosDescent.Scripts.resource.Abilities;

[GlobalClass]
public partial class DamageAbility : Ability
{
    [Export] public double BaseDamage { get; set; } = 20.0;
    [Export] public double Range { get; set; } = 150.0;
    [Export] public double AreaOfEffect { get; set; } // 0 means single target

    public DamageAbility()
    {
        Name = "Fireball";
        Description = "Launch a damaging projectile";
        Type = AbilityType.Active;
        Cooldown = 5.0;
    }

    protected override void ExecuteEffect(double powerMultiplier)
    {
        // Calculate damage with power multiplier
        var damage = BaseDamage * powerMultiplier;

        if (AreaOfEffect > 0)
        {
            // Area damage
            DealAreaDamage(damage);
        }
        else
        {
            // Single target damage
            DealSingleTargetDamage(damage);
        }

        GD.Print($"{Caster.Name} used {Name} dealing {damage} damage");
    }

    private void DealSingleTargetDamage(double damage)
    {
        // Find the closest entity within range
        var target = FindClosestEnemy();

        target?.TakeDamage(damage);
    }

    private void DealAreaDamage(double damage)
    {
        // Get all entities in the area
        var targets = Caster.GetTree().GetNodesInGroup("Entity");

        foreach (var node in targets)
        {
            if (node is not Entity target || target == Caster) continue;
            
            var distance = Caster.GlobalPosition.DistanceTo(target.GlobalPosition);

            // Check if entity is within range
            if (!(distance <= Range)) continue;
            
            // Apply damage with falloff based on distance
            var distanceFactor = 1.0 - distance / Range;
            target.TakeDamage(damage * distanceFactor);
        }
    }

    private Entity FindClosestEnemy()
    {
        Entity closest = null;
        var closestDistance = Range;

        var targets = Caster.GetTree().GetNodesInGroup("Entity");

        foreach (var node in targets)
        {
            if (node is not Entity target || target == Caster) continue;
            
            var distance = Caster.GlobalPosition.DistanceTo(target.GlobalPosition);

            if (!(distance < closestDistance)) continue;
            
            closestDistance = distance;
            closest = target;
        }

        return closest;
    }

    // For channeled version of the ability
    protected override void OnChannelingTick(double delta)
    {
        // Apply damage over time while channeling
        var tickDamage = BaseDamage * (delta / ChannelingDuration);

        if (AreaOfEffect > 0)
        {
            DealAreaDamage(tickDamage);
        }
        else
        {
            DealSingleTargetDamage(tickDamage);
        }
    }
}