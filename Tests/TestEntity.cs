using ChronosDescent.Scripts;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Animation;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Manager = ChronosDescent.Scripts.Core.Effect.Manager;

namespace ChronosDescent.Tests;

public partial class TestActionManager : ActionManager
{
    public new Vector2 LookDirection
    {
        get => base.LookDirection;
        set => base.LookDirection = value;
    }
}

public partial class TestEntity : CharacterBody2D, IEntity
{
    public Manager EffectManager { get; } = new();
    public Scripts.Core.Ability.Manager AbilityManager { get; } = new();
    public IAnimationPlayer AnimationManager => null;
    public Scripts.Core.Weapon.Manager WeaponManager { get; } = new();
    public AnimationPlayer WeaponAnimationPlayer => null;
    public EventBus EventBus { get; init; } = new();
    public IActionManager ActionManager { get; } = new TestActionManager();
    public Scripts.Core.State.Manager StatsManager { get; } = new(new EntityBaseStats());

    public new Vector2 GlobalPosition
    {
        get => base.Position;
        set => base.Position = value;
    }

    public bool Moveable { get; set; } = true;

    public bool Collision { get; set; } = true;
    public bool IsDead { get; set; }

    // IEntity implementations
    public void ApplyEffect(BaseEffect effect)
    {
        EffectManager.ApplyEffect(effect);
    }

    public void RemoveEffect(string effectId)
    {
        EffectManager.RemoveEffect(effectId);
    }

    public bool HasEffect(string effectId)
    {
        return EffectManager.HasEffect(effectId);
    }

    public void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        AbilityManager.SetAbility(slot, ability);
    }

    public void RemoveAbility(AbilitySlotType slot)
    {
        AbilityManager.RemoveAbility(slot);
    }

    public void TakeDamage(double amount, DamageType damageType)
    {
        // No-op for test implementation
    }

    public void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ActivateAbility(abilitySlotType);
    }

    public void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ReleaseAbility(abilitySlotType);
    }

    public void CancelAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.CancelAbility(abilitySlotType);
    }
    
    public override void _Ready()
    {
        AddToGroup("Entity");
        
        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
    }

    public bool CanActivateAbility(AbilitySlotType slot)
    {
        return AbilityManager.CanActivateAbility(slot);
    }
}