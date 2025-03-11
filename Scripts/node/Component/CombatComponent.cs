using System;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class CombatComponent : Node
{
    private AnimationComponent _animation;
    private StatsComponent _stats;

    public void Initialize(StatsComponent stats, AnimationComponent animation)
    {
        _stats = stats;
        _animation = animation;
    }

    public void TakeDamage(double amount)
    {
        if (_stats == null) return;


        _stats.Health -= amount;
        _animation?.PlayAnimation("hurt");

        if (_stats.Health <= 0) GetParent<Entity>().OnEntityDeath();
    }

    public void Heal(double amount)
    {
        if (_stats == null) return;

        _stats.Health = Math.Min(_stats.Health + amount, _stats.MaxHealth);
    }
}