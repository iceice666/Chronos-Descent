using ChronosDescent.Scripts;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

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
    public EventBus EventBus { get; init; } = new();
    public IActionManager ActionManager { get; } = new TestActionManager();
    public Scripts.Core.Stats.Manager StatsManager { get; } = new(new EntityBaseStats());
    public Scripts.Core.Effect.Manager EffectManager { get; } = new();
    public Scripts.Core.Ability.Manager AbilityManager { get; } = new();

    public new Vector2 Position
    {
        get => base.Position;
        set => base.Position = value;
    }

    private bool _moveable = true;
    public bool Moveable
    {
        get => _moveable;
        set => _moveable = value;
    }
    
    public bool Collision { get; set; } = true;

    public override void _Ready()
    {
        AddToGroup("Entity");

        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
    }

    // IEntity implementations
    public void ApplyEffect(IEffect effect) => EffectManager.ApplyEffect(effect);
    public void RemoveEffect(string effectId) => EffectManager.RemoveEffect(effectId);
    public bool HasEffect(string effectId) => EffectManager.HasEffect(effectId);

    public void SetAbility(AbilitySlotType slot, BaseAbility ability) => AbilityManager.SetAbility(slot, ability);
    public void RemoveAbility(AbilitySlotType slot) => AbilityManager.RemoveAbility(slot);
    public bool CanActivateAbility(AbilitySlotType slot) => AbilityManager.CanActivateAbility(slot);

    public void TakeDamage(double damage, DamageType damageType)
    {
        // No-op for test implementation
    }

    public void ActivateAbility(AbilitySlotType abilitySlotType) => AbilityManager.ActivateAbility(abilitySlotType);
    public void ReleaseAbility(AbilitySlotType abilitySlotType) => AbilityManager.ReleaseAbility(abilitySlotType);
    public void CancelAbility(AbilitySlotType abilitySlotType) => AbilityManager.CancelAbility(abilitySlotType);
}