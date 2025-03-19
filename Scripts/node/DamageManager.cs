using System;
using ChronosDescent.Scripts.Entity.Resource;
using Godot;


namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class DamageManager : Node
{
    public static DamageManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void DealDamage(BaseStats attackerStats, Entity.Entity attackee, double rawDamage,
        Entity.UI.DamageIndicator.DamageType? damageType = null)
    {
        if (attackee == null) return;

        // Base damage calculation
        var damage = rawDamage;

        // Calculate critical hit
        var isCritical = false;
        var critChance = attackerStats?.CriticalChance ?? 0;

        // Random check for critical hit
        if (GD.Randf() * 100 < critChance)
        {
            // Critical hit applies the critical damage multiplier
            var critMultiplier = attackerStats?.CriticalDamage / 100 ?? 0;
            damage *= (1 + critMultiplier);
            isCritical = true;
        }

        // Apply defense reduction (simple formula)
        var defenseMultiplier = 100 / (100 + attackee.Stats.Defense);
        damage *= defenseMultiplier;


        // Apply damage with appropriate visual indicator
        var dmgType = damageType ?? (isCritical
            ? Entity.UI.DamageIndicator.DamageType.Critical
            : Entity.UI.DamageIndicator.DamageType.Normal);

        attackee.TakeDamage(damage, dmgType);
    }
}