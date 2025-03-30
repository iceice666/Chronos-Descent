using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Tests;

public partial class TestActionManager : ActionManager
{
    public new Vector2 LookDirection
    {
        get => base.LookDirection;
        set => base.LookDirection = value;
    }
}

public partial class TestEntity : BaseEntity
{
    public override IActionManager ActionManager { get; protected set; } = new TestActionManager();
    public override Manager StatsManager { get; } = new(new EntityBaseStats());


    public override bool Collision { get; set; } = true;
    public bool IsDead { get; set; }

    // BaseEntity implementations
    public override void ApplyEffect(BaseEffect effect)
    {
        EffectManager.ApplyEffect(effect);
    }

    public override void RemoveEffect(string effectId)
    {
        EffectManager.RemoveEffect(effectId);
    }

    public override bool HasEffect(string effectId)
    {
        return EffectManager.HasEffect(effectId);
    }

    public override void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        AbilityManager.SetAbility(slot, ability);
    }

    public override void RemoveAbility(AbilitySlotType slot)
    {
        AbilityManager.RemoveAbility(slot);
    }

    public override void TakeDamage(double amount, DamageType damageType)
    {
        // No-op for test implementation
    }

    public override void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ActivateAbility(abilitySlotType);
    }

    public override void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ReleaseAbility(abilitySlotType);
    }

    public override void CancelAbility(AbilitySlotType abilitySlotType)
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