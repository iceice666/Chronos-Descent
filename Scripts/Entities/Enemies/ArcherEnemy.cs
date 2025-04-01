using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Weapons;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Scripts.Entities.Enemies;

[GlobalClass]
public partial class ArcherEnemy : BaseEnemy
{
    // Use RangedEnemyStats for archer enemies
    public override Manager StatsManager { get; } = new(new RangedEnemyStats());
    
    // Override to adjust archer's behavior
    private double _retreatTimer = 0.0;
    private const double RetreatDuration = 1.5;
    private bool _isRetreating = false;

    public override void _Ready()
    {
        base._Ready();

        // Set up the EnemyBow weapon
        WeaponManager.SetWeapon<EnemyBow>(GD.Load<PackedScene>("res://Scenes/weapon/enemy_bow.tscn"));
        
        // Ensure archer enemy only has the normal attack ability
        RemoveAbility(AbilitySlotType.Special);
        RemoveAbility(AbilitySlotType.Ultimate);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        WeaponMountPoint.Rotation = ActionManager.LookDirection.Angle();
        
        // Handle retreat timer if retreating
        if (!_isRetreating) return;
        _retreatTimer += delta;
        if (!(_retreatTimer >= RetreatDuration)) return;
        _isRetreating = false;
        _retreatTimer = 0.0;
    }

    protected override void UpdateState()
    {
        // First use the base state update logic to find/update target
        base.UpdateState();
        
        // If we have a target, add archer-specific behavior
        if (!CurrentTarget.IsDead)
        {
            var distanceToTarget = GlobalPosition.DistanceTo(CurrentTarget.GlobalPosition);
            
            // If too close to the target and not retreating, start retreat
            if (distanceToTarget < 100 && !_isRetreating && CurrentState != EnemyState.Retreat)
            {
                ChangeState(EnemyState.Retreat);
                _isRetreating = true;
                _retreatTimer = 0.0;
            }
            // If was retreating and now done, go back to appropriate state
            else if (!_isRetreating && CurrentState == EnemyState.Retreat)
            {
                if (distanceToTarget <= AttackRadius)
                    ChangeState(EnemyState.Attack);
                else if (distanceToTarget <= DetectionRadius)
                    ChangeState(EnemyState.Chase);
            }
        }
    }
    
    protected override void HandleMovement()
    {
        // If retreating, move away from the target instead of towards it
        if (CurrentState == EnemyState.Retreat)
        {
            // Calculate direction away from target
            var retreatDirection = (GlobalPosition - CurrentTarget.GlobalPosition).Normalized();
            
            // Move in retreat direction
            Velocity = retreatDirection * (float)StatsManager.MoveSpeed * 1.2f; // Slightly faster when retreating
            MoveAndSlide();
            
            // Update action manager
            var actionManager = (ActionManager.EnemyActionManager)ActionManager;
            actionManager.SetMoveDirection(retreatDirection);
        }
        else
        {
            // Use default movement for other states
            base.HandleMovement();
        }
    }

    // Override state transition animation
    protected override void OnStateTransitionStart(EnemyState fromState, EnemyState toState)
    {
        base.OnStateTransitionStart(fromState, toState);

        switch (toState)
        {
            case EnemyState.Chase:
            case EnemyState.Attack:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
                break;
            case EnemyState.Retreat:
                // Play retreat animation if you have one
                GetNode<AnimationPlayer>("AnimationPlayer").Play("walk");
                break;
        }
    }

    protected override void OnStateTransitionComplete(EnemyState fromState, EnemyState toState)
    {
        base.OnStateTransitionComplete(fromState, toState);
        
        // Reset any visual effects
        Sprite.Scale = Vector2.One;
        
        switch (toState)
        {
            case EnemyState.Chase:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("walk");
                break;
            case EnemyState.Idle:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
                break;
            case EnemyState.Attack:
                GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
                break;
        }
    }
    
    // Override to implement archer attack behavior
    public override void PerformAttack()
    {
        if (CurrentState == EnemyState.Attack)
        {
            ActivateAbility(AbilitySlotType.Normal);
        }
    }
    
    protected override void Die()
    {
        // Could spawn items, play death animation, etc.
        base.Die();
    }
}