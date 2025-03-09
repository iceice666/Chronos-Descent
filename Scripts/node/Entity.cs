using ChronosDescent.Scripts.node.Component;
using Godot;
using Effect = ChronosDescent.Scripts.resource.Effects.Effect;

namespace ChronosDescent.Scripts.node;

// Base Entity class that manages the components
[GlobalClass]
public partial class Entity : CharacterBody2D
{
    public AbilityManagerComponent AbilityManager;


    public Vector2 AimDirection;
    public AnimationComponent Animation;
    public CombatComponent Combat;
    public EffectManagerComponent EffectManager;

    public bool Moveable = true;

    // Component references
    public StatsComponent Stats;
    public TimeManipulationComponent TimeManipulation;

    public override void _Ready()
    {
        // Get component references
        Stats = GetNode<StatsComponent>("StatsComponent");
        EffectManager = GetNode<EffectManagerComponent>("EffectManagerComponent");
        Animation = GetNode<AnimationComponent>("AnimationComponent");
        Combat = GetNode<CombatComponent>("CombatComponent");
        TimeManipulation = GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        AbilityManager = GetNode<AbilityManagerComponent>("AbilityManagerComponent");

        // Setup component connections
        EffectManager.Initialize(Stats);
        Combat.Initialize(Stats, Animation);

        AddToGroup("Entity");
    }

    public override void _Process(double delta) { }

    public override void _PhysicsProcess(double delta)
    {
        // Movement logic would go here or in a separate MovementComponent
    }

    // Public methods that delegate to components
    public void TakeDamage(double amount)
    {
        Combat.TakeDamage(amount);
    }

    public void Heal(double amount)
    {
        Combat.Heal(amount);
    }

    #region Effect

    public void ApplyEffect(Effect effect)
    {
        EffectManager.ApplyEffect(effect);
    }

    public void RemoveEffect(string effectId)
    {
        EffectManager.RemoveEffect(effectId);
    }

    public void RemoveAllEffects()
    {
        EffectManager.RemoveAllEffects();
    }

    public bool HasEffect(string effectId)
    {
        return EffectManager.HasEffect(effectId);
    }

    #endregion


    #region Ability

    public void ActivateAbility(AbilityManagerComponent.Slot slot)
    {
        AbilityManager.ActivateAbility(slot);
    }

    public void ReleaseChargedAbility(AbilityManagerComponent.Slot slot)
    {
        AbilityManager.ReleaseChargedAbility(slot);
    }

    public void CancelChargedAbility(AbilityManagerComponent.Slot slot)
    {
        AbilityManager.CancelChargedAbility(slot);
    }

    public void InterruptChannelingAbility(AbilityManagerComponent.Slot slot)
    {
        AbilityManager.InterruptChannelingAbility(slot);
    }

    public bool IsAbilityReady(AbilityManagerComponent.Slot slot)
    {
        return AbilityManager.IsAbilityReady(slot);
    }

    public void ToggleAbility(AbilityManagerComponent.Slot slot)
    {
        AbilityManager.ToggleAbility(slot);
    }

    #endregion

    // Virtual method for derived classes to override
    public virtual void OnEntityDeath()
    {
        Combat.HandleDeath();
    }
}