using System;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class CombatComponent : Node
{
    private StatsComponent _stats;
    private AnimationComponent _animation;

    [Signal]
    public delegate void DeathEventHandler();

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

        if (_stats.Health <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(double amount)
    {
        if (_stats == null) return;
        
        _stats.Health =  Math.Min(_stats.Health + amount, _stats.MaxHealth);
    }

    public async void HandleDeath()
    {
        // Signal death event
        EmitSignal(SignalName.Death);

        // Play death animation
        _animation?.PlayAnimation("death");

        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        // Queue free the parent entity
        GetParent().QueueFree();
    }
}