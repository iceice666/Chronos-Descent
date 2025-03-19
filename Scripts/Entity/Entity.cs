using ChronosDescent.Scripts.Effect.Node;
using ChronosDescent.Scripts.Entity.Node;
using ChronosDescent.Scripts.Entity.UI;
using ChronosDescent.Scripts.Weapon;
using ChronosDescent.Scripts.Weapon.Node;
using Godot;
using AbilityManagerComponent = ChronosDescent.Scripts.Ability.Node.AbilityManagerComponent;
using AbilitySlot = ChronosDescent.Scripts.Ability.Node.AbilitySlot;

namespace ChronosDescent.Scripts.Entity;

// Base Entity class that manages the components
[GlobalClass]
public partial class Entity : CharacterBody2D
{
    private CollisionShape2D _collision;

    public Vector2 AimDirection;

    public bool Moveable { get; set; }= true;

    public override void _Ready()
    {
        // Get component references
        Stats = GetNode<StatsComponent>("StatsComponent");
        EffectManager = GetNode<EffectManagerComponent>("EffectManagerComponent");
        DamageIndicatorManager = GetNode<DamageIndicatorManagerComponent>(
            "DamageIndicatorManagerComponent"
        );
        Combat = GetNode<CombatComponent>("CombatComponent");

        TimeManipulation = GetNodeOrNull<TimeManipulationComponent>("TimeManipulationComponent");
        AbilityManager = GetNodeOrNull<AbilityManagerComponent>("AbilityManagerComponent");
        Animation = GetNodeOrNull<AnimationComponent>("AnimationComponent");
        Weapon = GetNodeOrNull<WeaponComponent>("WeaponComponent");

        _collision = GetNode<CollisionShape2D>("CollisionShape2D");

        // Setup component connections
        EffectManager.Initialize(Stats);
        Combat?.Initialize(Stats, Animation);

        Stats.EntityDead += OnEntityDeath;

        AddToGroup("Entity");
    }

    public override void _ExitTree()
    {
        Stats.EntityDead -= OnEntityDeath;
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movement logic would go here or in a separate MovementComponent
    }

    // Public methods that delegate to components
    public void TakeDamage(
        double amount,
        DamageIndicator.DamageType damageType = DamageIndicator.DamageType.Normal
    )
    {
        Combat.TakeDamage(amount, damageType);
    }

    public void Heal(double amount)
    {
        Combat.Heal(amount);
    }

    // Virtual method for derived classes to override
    public virtual async void OnEntityDeath()
    {
        // Play death animation
        Animation?.PlayAnimation("death");

        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        // Queue free the parent entity
        QueueFree();
    }

    public void DisableCollision(bool val)
    {
        _collision.Disabled = val;
    }

    // @formatter:off
    // Component references
    // Must have
    public StatsComponent Stats;
    public CombatComponent Combat;
    public DamageIndicatorManagerComponent DamageIndicatorManager;
    public EffectManagerComponent EffectManager;

    // Optional
    public TimeManipulationComponent TimeManipulation;
    public AnimationComponent Animation;
    public AbilityManagerComponent AbilityManager;
    public WeaponComponent Weapon;

    // @formatter:on

    #region Effect

    public void ApplyEffect(Effect.Effect effect)
    {
        EffectManager.ApplyEffect(effect);
    }

    public void RemoveEffect(string effectId)
    {
        EffectManager.RemoveEffect(effectId);
    }

    public void RemoveAllEffects()
    {
        EffectManager.RemoveAllEffects();
    }

    public bool HasEffect(string effectId)
    {
        return EffectManager.HasEffect(effectId);
    }

    #endregion
    
    #region Ability

    public void ActivateAbility(AbilitySlot abilitySlot)
    {
        if (AbilityManager == null)
        {
            Util.PrintWarning($"This entity({this}) has no ability manager!");
            return;
        }

        AbilityManager.ActivateAbility(abilitySlot);
    }

    public void ReleaseChargedAbility(AbilitySlot abilitySlot)
    {
        if (AbilityManager == null)
        {
            Util.PrintWarning($"This entity({this}) has no ability manager!");
            return;
        }

        AbilityManager.ReleaseChargedAbility(abilitySlot);
    }

    public void CancelChargedAbility()
    {
        if (AbilityManager == null)
        {
            Util.PrintWarning($"This entity({this}) has no ability manager!");
            return;
        }

        AbilityManager.CancelChargedAbility();
    }

    public void InterruptChannelingAbility()
    {
        if (AbilityManager == null)
        {
            Util.PrintWarning($"This entity({this}) has no ability manager!");
            return;
        }

        AbilityManager.InterruptChannelingAbility();
    }

    #endregion
    
    #region Weapon
    
    public void EquipWeapon(BaseWeapon weapon)
    {
        if (Weapon == null)
        {
            Util.PrintWarning($"This entity({this}) has no weapon component!");
            return;
        }
    
        Weapon.EquipWeapon(weapon);
    }

    public BaseWeapon GetCurrentWeapon()
    {
        return Weapon?.CurrentWeapon;
    }
    #endregion
}