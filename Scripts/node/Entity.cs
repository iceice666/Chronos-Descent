using System;
using Godot;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;

namespace ChronosDescent.Scripts.node;

// Base Entity class that manages the components
[GlobalClass]
public partial class Entity : CharacterBody2D
{
    // Component references
    public StatsComponent Stats;
    protected EffectManagerComponent EffectManager;
    protected AnimationComponent Animation;
    protected CombatComponent Combat;

    public override void _Ready()
    {
        // Get component references
        Stats = GetNode<StatsComponent>("StatsComponent");
        EffectManager = GetNode<EffectManagerComponent>("EffectManagerComponent");
        Animation = GetNode<AnimationComponent>("AnimationComponent");
        Combat = GetNode<CombatComponent>("CombatComponent");
        
        // Setup component connections
        EffectManager.Initialize(Stats);
        Combat.Initialize(Stats, Animation);
        
        AddToGroup("Entity");
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movement logic would go here or in a separate MovementComponent
    }
    
    // Public methods that delegate to components
    public void TakeDamage(double amount) => Combat.TakeDamage(amount);
    public void Heal(double amount) => Combat.Heal(amount);
    public void ApplyEffect(Effect effect) => EffectManager.ApplyEffect(effect);
    public void RemoveEffect(string effectName) => EffectManager.RemoveEffect(effectName);
    public void RemoveAllEffects() => EffectManager.RemoveAllEffects();
    public bool HasEffect(string effectName) => EffectManager.HasEffect(effectName);

    public void UpdateStats()
    {
        Stats.UpdateStats(stats =>
        {
            
        });
    }
    
    // Virtual method for derived classes to override
    public virtual void OnEntityDeath()
    {
        Combat.HandleDeath();
    }
}