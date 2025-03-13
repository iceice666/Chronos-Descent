using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.UI;
using Godot;
using Effect = ChronosDescent.Scripts.resource.Effect;

namespace ChronosDescent.Scripts.node;

// Base Entity class that manages the components
[GlobalClass]
public partial class Entity : CharacterBody2D
{
    public delegate void EntityDeathEventHandler(Entity entity);

    public AbilityManagerComponent AbilityManager;


    public Vector2 AimDirection;
    public AnimationComponent Animation;
    public CombatComponent Combat;
    public EffectManagerComponent EffectManager;
    public DamageIndicatorManagerComponent DamageIndicatorManager;

    public bool Moveable = true;

    // Component references
    public StatsComponent Stats;
    public TimeManipulationComponent TimeManipulation;
    public event EntityDeathEventHandler EntityDeath;

    protected virtual void OnEntityDeath(Entity entity)
    {
        EntityDeath?.Invoke(entity);
    }

    public override void _Ready()
    {
        // Get component references
        Stats = GetNode<StatsComponent>("StatsComponent");
        EffectManager = GetNode<EffectManagerComponent>("EffectManagerComponent");
        Animation = GetNode<AnimationComponent>("AnimationComponent");
        Combat = GetNode<CombatComponent>("CombatComponent");
        TimeManipulation = GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        AbilityManager = GetNode<AbilityManagerComponent>("AbilityManagerComponent");
        DamageIndicatorManager = GetNode<DamageIndicatorManagerComponent>("DamageIndicatorManagerComponent");

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
    public void TakeDamage(double amount, DamageIndicator.DamageType damageType = DamageIndicator.DamageType.Normal)
    {
        Combat.TakeDamage(amount, damageType);
    }

    public void Heal(double amount)
    {
        Combat.Heal(amount);
    }

    // Virtual method for derived classes to override
    public virtual async void OnEntityDeath()
    {
        // Signal death event
        OnEntityDeath(this);

        // Play death animation
        Animation.PlayAnimation("death");

        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        // Queue free the parent entity
        QueueFree();
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

    public void CancelChargedAbility()
    {
        AbilityManager.CancelChargedAbility();
    }

    public void InterruptChannelingAbility()
    {
        AbilityManager.InterruptChannelingAbility();
    }

    #endregion
}