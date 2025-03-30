using System;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Animation;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Entities;

public partial class Dummy : BaseEntity
{
    public IAnimationPlayer AnimationManager => null;

    public bool IsDead { get; set; }


    public override bool Collision { get; set; }

    public override void _Ready()
    {
        AddToGroup("Entity");


        EffectManager.Initialize(this);
    }

    public override void _Process(double delta)
    {
        EffectManager.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        EffectManager.FixedUpdate(delta);
    }

    public override void TakeDamage(double amount, DamageType damageType)
    {
        Indicator.Spawn(this, amount, damageType);
    }

    public override void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAbility(AbilitySlotType slot)
    {
        throw new NotImplementedException();
    }

    public override void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public override void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public override void CancelAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public override void ApplyEffect(BaseEffect effect)
    {
        throw new NotImplementedException();
    }

    public override void RemoveEffect(string effectId)
    {
        throw new NotImplementedException();
    }

    public override bool HasEffect(string effectId)
    {
        throw new NotImplementedException();
    }
}