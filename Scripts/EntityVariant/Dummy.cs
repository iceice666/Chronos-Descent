using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using Godot;
using Manager = ChronosDescent.Scripts.Core.Stats.Manager;

namespace ChronosDescent.Scripts.EntityVariant;

public partial class Dummy : CharacterBody2D, IEntity
{
    public EventBus EventBus { get; init; } = new ();
    public IActionManager ActionManager { get; }
    public Manager StatsManager { get; }

    public new Vector2 Position
    {
        get => base.Position;
        set => base.Position = value;
    }

    public bool Moveable { get; set; } 
    public bool Collision { get; set; }

    public void TakeDamage(double damage, DamageType damageType)
    {
        throw new System.NotImplementedException();
    }

    public void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAbility(AbilitySlotType slot)
    {
        throw new System.NotImplementedException();
    }

    public void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        throw new System.NotImplementedException();
    }

    public void CancelAbility(AbilitySlotType abilitySlotType)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyEffect(IEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveEffect(string effectId)
    {
        throw new System.NotImplementedException();
    }

    public bool HasEffect(string effectId)
    {
        throw new System.NotImplementedException();
    }
}