using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Animation;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.UI;
using ChronosDescent.Scripts.Weapons;
using Godot;

namespace ChronosDescent.Scripts.Entities;

[GlobalClass]
public partial class Player : CharacterBody2D, IEntity
{
    #region Components

    private Area2D _effectBox;
    private CollisionShape2D _hurtBox;

    public EventBus EventBus { get; init; } = new();
    public Core.State.Manager StatsManager { get; } = new(new EntityBaseStats());
    public Core.Ability.Manager AbilityManager { get; } = new();
    protected Core.Effect.Manager EffectManager = new();
    public IAnimationPlayer AnimationManager { get; } = new PlayerAnimationManager();
    public Core.Weapon.Manager WeaponManager { get; } = new();
    public AnimationPlayer WeaponAnimationPlayer { get; private set; }

    #endregion

    private int _moveableCounter;
    private Node2D _weaponMountPoint;

    public bool IsDead { get; private set; }


    public bool Collision
    {
        get => !_hurtBox.Disabled;
        set => _hurtBox.Disabled = !value;
    }


    public IActionManager ActionManager { get; private set; }

    public new Vector2 GlobalPosition
    {
        get => base.GlobalPosition;
        set => base.GlobalPosition = value;
    }

    public bool Moveable
    {
        get => _moveableCounter <= 0;
        set => _moveableCounter = Mathf.Max(_moveableCounter + (value ? 1 : -1), 0);
    }

    public override void _Ready()
    {
        AddToGroup("Entity");
        AddToGroup("Player");

        _hurtBox = GetNode<CollisionShape2D>("HurtBox");
        _effectBox = GetNode<Area2D>("EffectBox");
        ActionManager = GetNode<UserInputManager>("/root/Autoload/UserInputManager");
        _weaponMountPoint = GetNode<Node2D>("WeaponMountPoint");
        WeaponAnimationPlayer = GetNode<AnimationPlayer>("WeaponAnimationPlayer");


        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
        WeaponManager.Initialize(this);


        GetNode<Camera>("/root/Autoload/Camera").Initialize(this);
        GetNode<PlayerHealthBar>("/root/Autoload/UI/PlayerHealthBar").Initialize(this);


        WeaponManager.SetWeapon<Weapons.Claymore>(GD.Load<PackedScene>("res://Scenes/weapon/claymore.tscn"));
        //    WeaponManager.SetWeapon<Bow>(GD.Load<PackedScene>("res://Scenes/weapon/bow.tscn"));
    }

    public override void _ExitTree()
    {
    }


    public override void _Process(double delta)
    {
        StatsManager.Update(delta);
        AbilityManager.Update(delta);
        EffectManager.Update(delta);
        WeaponManager.Update(delta);

        if (_weaponMountPoint.GetChild(0)?.IsInGroup("NeedRotation") ?? false)
        {
            _weaponMountPoint.Rotation =
                Mathf.Atan2(ActionManager.LookDirection.Y, float.Abs(ActionManager.LookDirection.X));
        }
    }

    private AbilitySlotType[] _abilitySlots =
    [
        AbilitySlotType.Normal,
        AbilitySlotType.Special,
        AbilitySlotType.Ultimate,
        AbilitySlotType.LifeSaving
    ];

    public override void _PhysicsProcess(double delta)
    {
        StatsManager.FixedUpdate(delta);
        AbilityManager.FixedUpdate(delta);
        EffectManager.FixedUpdate(delta);
        WeaponManager.FixedUpdate(delta);

        Velocity = ActionManager.MoveDirection * (float)StatsManager.MoveSpeed;
        MoveAndSlide();


        foreach (var slotType in _abilitySlots)
        {
            var slotName = slotType.GetSlotName();

            if (Input.IsActionJustPressed(slotName))
                ActivateAbility(slotType);
            else if (Input.IsActionJustReleased(slotName))
                ReleaseAbility(slotType);
        }
    }


    #region Effect

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

    #endregion


    public void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        AbilityManager.SetAbility(slot, ability);
    }

    public void RemoveAbility(AbilitySlotType slot)
    {
        AbilityManager.RemoveAbility(slot);
    }

    public bool CanActivateAbility(AbilitySlotType slot)
    {
        return AbilityManager.CanActivateAbility(slot);
    }

    private PackedScene _indicatorScene = GD.Load<PackedScene>("res://Scenes/ui/damage_indicator.tscn");

    public void TakeDamage(double amount, DamageType damageType)
    {
        // Update health
        StatsManager.Health += damageType == DamageType.Healing ? amount : -amount;

        // Emit event for damage taken
        EventBus.Publish(EventVariant.EntityStatChanged);

        // Check if entity died
        if (StatsManager.CurrentStats.Health <= 0)
        {
            _moveableCounter = 0;

            EventBus.Publish(EventVariant.EntityDied);
            GlobalEventBus.Instance.Publish<IEntity>(GlobalEventVariant.EntityDied, this);
            IsDead = true;
        }

        // Display damage indicator
        Indicator.Spawn(this, amount, damageType);
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
}