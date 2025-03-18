using System;
using ChronosDescent.Scripts.UI;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class CombatComponent : Node
{
    private AnimationComponent _animation;
    private Entity _owner;
    private StatsComponent _stats;

    public void Initialize(StatsComponent stats, AnimationComponent animation)
    {
        _stats = stats;
        _animation = animation;
        _owner = (Entity)Owner;
    }

    public void TakeDamage(double amount,
        DamageIndicator.DamageType damageType)
    {
        if (_stats == null) return;

        // Calculate actual damage (could add defense calculations here)
        _stats.Health -= amount;
        _animation?.PlayAnimation("hurt");

        // Show damage indicator
        _owner.DamageIndicatorManager.ShowDamageIndicator(
            _owner.GlobalPosition,
            amount,
            damageType
        );


        if (_stats.Health <= 0) GetParent<Entity>().OnEntityDeath();
    }


    public void Heal(double amount)
    {
        if (_stats == null) return;

        var previousHealth = _stats.Health;
        _stats.Health = Math.Min(_stats.Health + amount, _stats.MaxHealth);

        // Calculate how much was actually healed
        var actualHealAmount = _stats.Health - previousHealth;

        // Show healing indicator (only if actually healed)
        if (actualHealAmount > 0)
            _owner.DamageIndicatorManager.ShowDamageIndicator(
                _owner.GlobalPosition,
                actualHealAmount,
                DamageIndicator.DamageType.Healing
            );
    }
}