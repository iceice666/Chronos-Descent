using System;
using ChronosDescent.Scripts.Abilities;
using ChronosDescent.Scripts.ActionManager;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Animation;
using ChronosDescent.Scripts.Core.Blessing;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.UI;
using ChronosDescent.Scripts.Weapons;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Scripts.Entities;

[GlobalClass]
public partial class Player : BaseEntity
{
    private readonly AbilitySlotType[] _abilitySlots =
    [
        AbilitySlotType.Normal,
        AbilitySlotType.Special,
        AbilitySlotType.Ultimate,
        AbilitySlotType.LifeSaving
    ];

    private PackedScene _indicatorScene = GD.Load<PackedScene>("res://Scenes/ui/damage_indicator.tscn");

    private bool _isPrevLookRight;


    private Node2D _weaponMountPoint;


    public override bool Collision
    {
        get => !_collision.Disabled;
        set => _collision.Disabled = !value;
    }


    public override void _Ready()
    {
        AddToGroup("Entity");
        AddToGroup("Player");

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _hurtbox = GetNode<Hurtbox>("Hurtbox");
        ActionManager = GetNode<UserInputManager>("../UserInputManager");
        _weaponMountPoint = GetNode<Node2D>("WeaponMountPoint");
        WeaponAnimationPlayer = GetNode<AnimationPlayer>("WeaponAnimationPlayer");
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");


        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
        WeaponManager.Initialize(this);
        PositionRecord.Initialize(this);
        CurrencyManager.Initialize(this);
        BlessingManager.Initialize(this);

        // Initialize game stats
        GameStats.Instance.StartGame();

        // Give starting currency
        CurrencyManager.SetChronoshards(10);


        GetNode<Camera>("../Camera").Initialize(this);
        GetNode<PlayerHealthBar>("../UI/TopLeftContainer/PlayerHealthBar").Initialize(this);
        GetNode<CurrencyDisplay>("../UI/TopLeftContainer/CurrencyDisplay").Initialize(this);

        // Check if we have player config from preparation room
        ApplyPlayerConfiguration();
    }

    public override void _ExitTree()
    {
    }

    private void ApplyPlayerConfiguration()
    {
        var gameManager = GetNode<GameManager>("/root/GameManager");

        // Apply weapon selection
        switch (gameManager.SelectedWeapon)
        {
            case "Weapon_Bow":
                WeaponManager.SetWeapon<Bow>(GD.Load<PackedScene>("res://Scenes/weapon/bow.tscn"));
                break;
            case "Weapon_Claymore":
                WeaponManager.SetWeapon<Claymore>(GD.Load<PackedScene>("res://Scenes/weapon/claymore.tscn"));
                break;
        }

        // Apply ability selection
        BaseAbility ability = gameManager.SelectedAbility switch
        {
            "LifeSaving_Dash" => new Dash(),
            "LifeSaving_Time Rewind" => new TimeRewind(),
            "LifeSaving_Heal" => new Heal(),
            _ => null
        };

        SetAbility(AbilitySlotType.LifeSaving, ability);
    }

    public override void _Process(double delta)
    {
        StatsManager.Update(delta);
        AbilityManager.Update(delta);
        EffectManager.Update(delta);
        WeaponManager.Update(delta);
        PositionRecord.Update(delta);
        CurrencyManager.Update(delta);
        BlessingManager.Update(delta);

        var isLookRight = ActionManager.LookDirection.X < 0;
        if (_isPrevLookRight != isLookRight)
        {
            Scale *= new Vector2(-1, 1);
            _isPrevLookRight = isLookRight;
        }

        if (_weaponMountPoint.GetChild(0)?.IsInGroup("NeedRotation") ?? false)
            _weaponMountPoint.Rotation =
                Mathf.Atan2(ActionManager.LookDirection.Y, Math.Abs(ActionManager.LookDirection.X));
    }

    public override void _PhysicsProcess(double delta)
    {
        StatsManager.FixedUpdate(delta);
        AbilityManager.FixedUpdate(delta);
        EffectManager.FixedUpdate(delta);
        WeaponManager.FixedUpdate(delta);
        PositionRecord.FixedUpdate(delta);
        CurrencyManager.FixedUpdate(delta);
        BlessingManager.FixedUpdate(delta);

        if (Moveable)
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

    public override void TakeDamage(double amount, DamageType damageType, Vector2 knockback = new())
    {
        // Allow blessings to react to damage before it's applied
        if (damageType != DamageType.Healing) BlessingManager.NotifyDamageTaken(amount);

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

            // Trigger game over event
            GlobalEventBus.Instance.Publish(GlobalEventVariant.GameOver);

            IsDead = true;
        }

        // Display damage indicator
        Indicator.Spawn(this, amount, damageType);

        // Apply knockback
        Velocity += knockback;
    }

    public override void ActivateAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ActivateAbility(abilitySlotType);

        // Notify blessings about ability usage
        var abilityVariant = abilitySlotType switch
        {
            AbilitySlotType.Normal => AbilityVariant.Normal,
            AbilitySlotType.Special => AbilityVariant.Special,
            AbilitySlotType.Ultimate => AbilityVariant.Ultimate,
            AbilitySlotType.LifeSaving => GetAbilityVariant(abilitySlotType),
            _ => AbilityVariant.Other
        };

        BlessingManager.NotifyAbilityUsed(abilityVariant);
    }

    // Helper method to determine ability variant for life-saving abilities
    private AbilityVariant GetAbilityVariant(AbilitySlotType slotType)
    {
        var ability = AbilityManager.GetAbility(slotType);

        if (ability == null)
            return AbilityVariant.Other;

        return ability.Id switch
        {
            "dash" => AbilityVariant.Dash,
            "time_rewind" => AbilityVariant.TimeRewind,
            _ => AbilityVariant.Other
        };
    }

    public override void ReleaseAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.ReleaseAbility(abilitySlotType);
    }

    public override void CancelAbility(AbilitySlotType abilitySlotType)
    {
        AbilityManager.CancelAbility(abilitySlotType);
    }

    #region Components

    private Hurtbox _hurtbox;
    private Sprite2D _sprite;
    private CollisionShape2D _collision;

    public override Manager StatsManager { get; } = new(new EntityBaseStats());
    public IAnimationPlayer AnimationManager { get; } = new PlayerAnimationManager();
    public PositionRecord PositionRecord { get; set; } = new();

    #endregion


    #region Effect

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

    #endregion
}