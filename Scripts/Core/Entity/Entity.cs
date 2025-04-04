using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Currency;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Scripts.Core.Entity;

public interface ISystem
{
    public void Initialize(BaseEntity owner);
    public void Update(double delta);
    public void FixedUpdate(double delta);
}

public abstract partial class BaseEntity : CharacterBody2D
{
    protected int MoveableCounter;
    public EventBus EventBus { get; } = new();
    public virtual IActionManager ActionManager { get; protected set; }
    public virtual Manager StatsManager { get; }
    public Ability.Manager AbilityManager { get; } = new();
    public Weapon.Manager WeaponManager { get; } = new();
    public Effect.Manager EffectManager { get; } = new();
    public CurrencyManager CurrencyManager { get; } = new();
    public Blessing.Manager BlessingManager { get; } = new();
    public AnimationPlayer WeaponAnimationPlayer { get; protected set; }

    public bool Moveable
    {
        get => MoveableCounter <= 0;
        set => MoveableCounter = Mathf.Max(MoveableCounter + (value ? -1 : 1), 0);
    }

    public abstract bool Collision { get; set; }

    public bool IsDead { get; protected set; }

    // Stats
    public abstract void TakeDamage(double amount, DamageType damageType, Vector2 knockback = new());

    // Ability
    public abstract void SetAbility(AbilitySlotType slot, BaseAbility ability);
    public abstract void RemoveAbility(AbilitySlotType slot);
    public abstract void ActivateAbility(AbilitySlotType abilitySlotType);
    public abstract void ReleaseAbility(AbilitySlotType abilitySlotType);
    public abstract void CancelAbility(AbilitySlotType abilitySlotType);

    // Effect
    public abstract void ApplyEffect(BaseEffect effect);
    public abstract void RemoveEffect(string effectId);
    public abstract bool HasEffect(string effectId);
}