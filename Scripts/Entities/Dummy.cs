using System;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Animation;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Manager = ChronosDescent.Scripts.Core.Weapon.Manager;

namespace ChronosDescent.Scripts.Entities;

public partial class Dummy : CharacterBody2D, IEntity
{
    public EventBus EventBus { get; init; } = new();
    public IActionManager ActionManager => null;
    public Core.State.Manager StatsManager => null;
    public Core.Ability.Manager AbilityManager => null;
    public IAnimationPlayer AnimationManager => null;
    public Manager WeaponManager => null;
    public AnimationPlayer WeaponAnimationPlayer => null;

    public new Vector2 GlobalPosition
    {
        get => base.Position;
        set => base.Position = value;
    }

    public bool Moveable { get; set; }
    public bool Collision { get; set; }
    public bool IsDead { get; set; }

    public override void _Ready()
    {
        AddToGroup("Entity");
    }

   

    public void TakeDamage(double amount, DamageType damageType)
    {
        Indicator.Spawn(this, amount, damageType);
    }
    
    public void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        throw new NotImplementedException();
    }

    public void RemoveAbility(AbilitySlotType slot)
    {
        throw new NotImplementedException();
    }

    public void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public void CancelAbility(AbilitySlotType abilitySlotType)
    {
        throw new NotImplementedException();
    }

    public void ApplyEffect(BaseEffect effect)
    {
        throw new NotImplementedException();
    }

    public void RemoveEffect(string effectId)
    {
        throw new NotImplementedException();
    }

    public bool HasEffect(string effectId)
    {
        throw new NotImplementedException();
    }
    
    
}