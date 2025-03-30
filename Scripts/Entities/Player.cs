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
public partial class Player : BaseEntity
{
    #region Components

    private Area2D _effectBox;
    private CollisionShape2D _hurtBox;
    private Sprite2D _sprite;

    public override Core.State.Manager StatsManager { get; } = new(new EntityBaseStats());
    public IAnimationPlayer AnimationManager { get; } = new PlayerAnimationManager();
    public PositionRecord PositionRecord { get; set; } = new();

    #endregion


    private Node2D _weaponMountPoint;

    public bool IsDead { get; private set; }


    public override bool Collision
    {
        get => !_hurtBox.Disabled;
        set => _hurtBox.Disabled = !value;
    }


  


    public override void _Ready()
    {
        AddToGroup("Entity");
        AddToGroup("Player");

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _hurtBox = GetNode<CollisionShape2D>("HurtBox");
        _effectBox = GetNode<Area2D>("EffectBox");
        ActionManager = GetNode<UserInputManager>("/root/Autoload/UserInputManager");
        _weaponMountPoint = GetNode<Node2D>("WeaponMountPoint");
        WeaponAnimationPlayer = GetNode<AnimationPlayer>("WeaponAnimationPlayer");


        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
        WeaponManager.Initialize(this);
        PositionRecord.Initialize(this);


        GetNode<Camera>("/root/Autoload/Camera").Initialize(this);
        GetNode<PlayerHealthBar>("/root/Autoload/UI/PlayerHealthBar").Initialize(this);


        WeaponManager.SetWeapon<Claymore>(GD.Load<PackedScene>("res://Scenes/weapon/claymore.tscn"));
        // WeaponManager.SetWeapon<Bow>(GD.Load<PackedScene>("res://Scenes/weapon/bow.tscn"));
    }

    public override void _ExitTree()
    {
    }

    private bool _isPrevLookRight;

    public override void _Process(double delta)
    {
        StatsManager.Update(delta);
        AbilityManager.Update(delta);
        EffectManager.Update(delta);
        WeaponManager.Update(delta);
        PositionRecord.Update(delta);

        var isLookRight = ActionManager.LookDirection.X < 0;
        if (_isPrevLookRight != isLookRight)
        {
            Scale *= new Vector2(-1, 1);
            _isPrevLookRight = isLookRight;
        }

        if (_weaponMountPoint.GetChild(0)?.IsInGroup("NeedRotation") ?? false)
        {
            _weaponMountPoint.Rotation = Mathf.Atan2(ActionManager.LookDirection.Y, ActionManager.LookDirection.X);
        }
    }

    private readonly AbilitySlotType[] _abilitySlots =
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
        PositionRecord.FixedUpdate(delta);

        if (!Moveable)
        {
            Velocity = ActionManager.MoveDirection * (float)StatsManager.MoveSpeed;
            MoveAndSlide();
        }


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

    public override void ApplyEffect(BaseEffect effect) => EffectManager.ApplyEffect(effect);
    public override void RemoveEffect(string effectId) => EffectManager.RemoveEffect(effectId);
    public override bool HasEffect(string effectId) => EffectManager.HasEffect(effectId);

    #endregion


    public override void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        AbilityManager.SetAbility(slot, ability);
    }

    public override void RemoveAbility(AbilitySlotType slot)
    {
        AbilityManager.RemoveAbility(slot);
    }

    public bool CanActivateAbility(AbilitySlotType slot)
    {
        return AbilityManager.CanActivateAbility(slot);
    }

    private PackedScene _indicatorScene = GD.Load<PackedScene>("res://Scenes/ui/damage_indicator.tscn");

    public override void TakeDamage(double amount, DamageType damageType)
    {
        // Update health
        StatsManager.Health += damageType == DamageType.Healing ? amount : -amount;

        // Emit event for damage taken
        EventBus.Publish(EventVariant.EntityStatChanged);

        // Check if entity died
        if (StatsManager.CurrentStats.Health <= 0)
        {
            MoveableCounter = 0;

            EventBus.Publish(EventVariant.EntityDied);
            GlobalEventBus.Instance.Publish<BaseEntity>(GlobalEventVariant.EntityDied, this);
            IsDead = true;
        }

        // Display damage indicator
        Indicator.Spawn(this, amount, damageType);
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
}