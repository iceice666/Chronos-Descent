using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using Godot;

namespace ChronosDescent.Scripts.Core.Entity;

public interface IManager
{
    public void Initialize(IEntity owner);
    public void Update(double delta);
    public void FixedUpdate(double delta);
}

public interface IEntity
{
    public EventBus EventBus { get; init; }
    public IActionManager ActionManager { get; }
    public Stats.Manager StatsManager { get; }

    public Vector2 Position { get; set; }
    public bool Moveable { get; set; }
    public bool Collision {get;set;}

    // Stats
    public void TakeDamage(double damage, DamageType damageType);

    // Ability
    public void SetAbility(AbilitySlotType slot, BaseAbility ability);
    public void RemoveAbility(AbilitySlotType slot);
    public void ActivateAbility(AbilitySlotType abilitySlotType);
    public void ReleaseAbility(AbilitySlotType abilitySlotType);
    public void CancelAbility(AbilitySlotType abilitySlotType);

    // Effect
    public void ApplyEffect(IEffect effect);
    public void RemoveEffect(string effectId);
    public bool HasEffect(string effectId);
}