using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.EntityVariant;

public partial class Player : CharacterBody2D, IEntity
{
    #region Components

    public EventBus EventBus { get; init; } = new();
    public Core.Stats.Manager StatsManager { get; } = new(new Core.Stats.EntityBaseStats());
    protected Core.Ability.Manager AbilityManager = new();
    protected Core.Effect.Manager EffectManager = new();

    #endregion

    private CollisionShape2D _hurtBox;
    private Area2D _effectBox;

    public bool Collision
    {
        get => !_hurtBox.Disabled;
        set => _hurtBox.Disabled = !value;
    }

    public IActionManager ActionManager => throw new System.NotImplementedException();

    public new Vector2 Position
    {
        get => base.Position;
        set => base.Position = value;
    }

    private int _moveableCounter;

    public bool Moveable
    {
        get => _moveableCounter <= 0;
        set => _moveableCounter += value ? 1 : -1;
    }

    public override void _Ready()
    {
        AddToGroup("Entity");

        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);

        _hurtBox = GetNode<CollisionShape2D>("HurtBox");
        _effectBox = GetNode<Area2D>("EffectBox");
    }

    public override void _ExitTree()
    {
    }


    public override void _Process(double delta)
    {
        StatsManager.Update(delta);
        AbilityManager.Update(delta);
        EffectManager.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        StatsManager.FixedUpdate(delta);
        AbilityManager.FixedUpdate(delta);
        EffectManager.FixedUpdate(delta);
    }

    #region IEntity interface
    public void ApplyEffect(IEffect effect) => EffectManager.ApplyEffect(effect);
    public void RemoveEffect(string effectId) => EffectManager.RemoveEffect(effectId);
    public bool HasEffect(string effectId) => EffectManager.HasEffect(effectId);

    public void SetAbility(AbilitySlotType slot, BaseAbility ability) => AbilityManager.SetAbility(slot, ability);
    public void RemoveAbility(AbilitySlotType slot) => AbilityManager.RemoveAbility(slot);
    public bool CanActivateAbility(AbilitySlotType slot) => AbilityManager.CanActivateAbility(slot);

    public void TakeDamage(double damage, DamageType damageType)
    {
        throw new System.NotImplementedException();
    }

    public void ActivateAbility(AbilitySlotType abilitySlotType) => AbilityManager.ActivateAbility(abilitySlotType);
    public void ReleaseAbility(AbilitySlotType abilitySlotType) => AbilityManager.ReleaseAbility(abilitySlotType);
    public void CancelAbility(AbilitySlotType abilitySlotType) => AbilityManager.CancelAbility(abilitySlotType);

    #endregion

    #region Event Callback

    #endregion
}