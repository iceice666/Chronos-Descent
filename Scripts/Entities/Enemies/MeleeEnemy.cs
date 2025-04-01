using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.Weapons;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Scripts.Entities.Enemies;

[GlobalClass]
public partial class MeleeEnemy : BaseEnemy
{
    public override Manager StatsManager { get; } = new(new MeleeEnemyStats());

    public override void _Ready()
    {
        base._Ready();

        // Set up a melee weapon with EnemyClaymore instead of player Claymore
        WeaponManager.SetWeapon<EnemyClaymore>(GD.Load<PackedScene>("res://Scenes/weapon/enemy_claymore.tscn"));

        // Ensure melee enemy only has the normal attack ability
        RemoveAbility(AbilitySlotType.Special);
        RemoveAbility(AbilitySlotType.Ultimate);
    }

    // Add visual feedback for melee enemy state transitions
    protected override void OnStateTransitionStart(EnemyState fromState, EnemyState toState)
    {
        base.OnStateTransitionStart(fromState, toState);

        switch (toState)
        {
            // For example, play a preparation animation based on the target state
            case EnemyState.Chase:
            // Play preparation stance
            case EnemyState.Attack:
                // Play readying animation or effect
                GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
                break;
        }
    }

    protected override void OnStateTransitionUpdate(double progress)
    {
        base.OnStateTransitionUpdate(progress);

        // Could scale or pulse the sprite during transition
        {
            // Pulse effect - scale between 0.9 and 1.1 during transition
            var scaleValue = 1.0f + 0.1f * Mathf.Sin((float)(progress * Mathf.Pi * 2));
            Sprite.Scale = new Vector2(scaleValue, scaleValue);
        }
    }

    protected override void OnStateTransitionComplete(EnemyState fromState, EnemyState toState)
    {
        base.OnStateTransitionComplete(fromState, toState);

        // Reset any visual effects
        Sprite.Scale = Vector2.One;

        switch (toState)
        {
            // Play state-specific animation
            case EnemyState.Chase:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("walk");
                break;
            case EnemyState.Idle:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
                break;
        }
    }

    public override void PerformAttack()
    {
        // Melee enemies prioritize the normal attack
        ActivateAbility(AbilitySlotType.Normal);
    }

    protected override void Die()
    {
        // Could spawn items, play death animation, etc.
        base.Die();
    }
}