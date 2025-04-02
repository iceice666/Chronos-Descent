using ChronosDescent.Scripts.ActionManager;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.Items;
using Godot;
using Manager = ChronosDescent.Scripts.Core.State.Manager;

namespace ChronosDescent.Scripts.Entities.Enemies;

[GlobalClass]
public partial class BaseEnemy : BaseEntity
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Retreat
    }

    protected const double AttackCooldown = 1.0;
    protected double AttackCooldownTimer = 0.0;
    protected bool CanAttack = true;
    protected CollisionShape2D CollisionObject;

    protected EnemyBaseStats EnemyStats;
    protected Hurtbox Hurtbox;
    protected NavigationAgent2D NavigationAgent;
    protected Sprite2D Sprite;
    protected Node2D WeaponMountPoint;

    public BaseEnemy()
    {
        AttackRadius = (float)((EnemyBaseStats)StatsManager.BaseStats).AttackRadius;
        DetectionRadius = (float)((EnemyBaseStats)StatsManager.BaseStats).DetectionRadius;
    }

    // Currency drop settings
    [Export] protected int MinCurrencyDrop { get; set; } = 5;
    [Export] protected int MaxCurrencyDrop { get; set; } = 15;
    [Export] protected float CurrencyDropChance { get; set; } = 0.75f; // 75% chance to drop currency
    [Export] protected PackedScene ChronoshardScene { get; set; }

    public EnemyState CurrentState { get; protected set; } = EnemyState.Idle;
    public BaseEntity CurrentTarget { get; protected set; }

    public override bool Collision
    {
        get => !CollisionObject.Disabled;
        set => CollisionObject.Disabled = !value;
    }

    public override Manager StatsManager { get; } = new(new EnemyBaseStats());

    public float AttackRadius { get; set; }

    public float DetectionRadius { get; set; }

    public override void _Ready()
    {
        AddToGroup("Entity");
        AddToGroup("Enemy");

        Sprite = GetNode<Sprite2D>("Sprite2D");
        Hurtbox = GetNode<Hurtbox>("Hurtbox");
        CollisionObject = GetNode<CollisionShape2D>("CollisionShape2D");
        NavigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        WeaponMountPoint = GetNode<Node2D>("WeaponMountPoint");
        WeaponAnimationPlayer = GetNodeOrNull<AnimationPlayer>("WeaponAnimationPlayer");

        ActionManager = GetNode<EnemyActionManager>("EnemyActionManager");

        StatsManager.Initialize(this);
        AbilityManager.Initialize(this);
        EffectManager.Initialize(this);
        WeaponManager.Initialize(this);

        NavigationAgent.MaxSpeed = (float)StatsManager.MoveSpeed;
        NavigationAgent.PathDesiredDistance = 4.0f;
        NavigationAgent.TargetDesiredDistance = 4.0f;
    }

    public override void _Process(double delta)
    {
        StatsManager.Update(delta);
        AbilityManager.Update(delta);
        EffectManager.Update(delta);
        WeaponManager.Update(delta);

        // Handle state transitions
        UpdateState();

        // Update look direction
        UpdateLookDirection();
    }

    public override void _PhysicsProcess(double delta)
    {
        StatsManager.FixedUpdate(delta);
        AbilityManager.FixedUpdate(delta);
        EffectManager.FixedUpdate(delta);
        WeaponManager.FixedUpdate(delta);

        if (IsDead) return;

        if (Moveable && CurrentState != EnemyState.Idle) HandleMovement();
    }

    protected virtual void UpdateState()
    {
        // Find the nearest player if we don't have a target
        if (CurrentTarget?.IsDead ?? true)
        {
            FindTarget();
        }
        else
        {
            var distanceToTarget = GlobalPosition.DistanceTo(CurrentTarget.GlobalPosition);

            if (distanceToTarget <= AttackRadius)
            {
                ChangeState(EnemyState.Attack);
            }
            else if (distanceToTarget <= DetectionRadius)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Idle);
                CurrentTarget = null;
            }
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return;

        // Handle exit state logic
        switch (CurrentState)
        {
            case EnemyState.Attack:
                CancelAbility(AbilitySlotType.Normal);
                break;
        }

        // Update the current state
        var previousState = CurrentState;
        CurrentState = newState;

        // Call transition start callback
        OnStateTransitionStart(previousState, newState);

        // Handle enter state logic
        switch (CurrentState)
        {
            case EnemyState.Chase:
                if (CurrentTarget != null) NavigationAgent.TargetPosition = CurrentTarget.GlobalPosition;

                break;
        }

        // Call transition complete callback
        OnStateTransitionComplete(previousState, newState);
    }

    // State transition callbacks - these can be overridden in derived enemy classes
    protected virtual void OnStateTransitionStart(EnemyState fromState, EnemyState toState)
    {
        // Default implementation does nothing
    }

    protected virtual void OnStateTransitionUpdate(double progress)
    {
        // Default implementation does nothing
    }

    protected virtual void OnStateTransitionComplete(EnemyState fromState, EnemyState toState)
    {
        // Default implementation does nothing
    }

    protected virtual void FindTarget()
    {
        // First try to get the player through more efficient group query
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
            foreach (var player in players)
            {
                if (player is not Node2D playerNode) continue;

                var distanceToPlayer = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
                if (!(distanceToPlayer <= DetectionRadius)) continue;
                // Check line of sight if needed
                if (!HasLineOfSightTo(playerNode)) continue;
                CurrentTarget = (BaseEntity)playerNode;
                return;
            }

        // Fallback to physics shape query if needed for more complex scenarios
        var space = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D();
        var shape = new CircleShape2D();
        shape.Radius = DetectionRadius;

        query.Shape = shape;
        query.Transform = Transform2D.Identity with { Origin = GlobalPosition };
        query.CollisionMask = 1 << 2; // Player collision layer

        var results = space.IntersectShape(query);

        if (results.Count <= 0) return;
        foreach (var result in results)
        {
            var collider = result["collider"].As<Node2D>();
            if (!collider.IsInGroup("Player")) continue;
            CurrentTarget = (BaseEntity)collider;
            break;
        }
    }

    protected virtual bool HasLineOfSightTo(Node2D target)
    {
        // Cast a ray to check if there's a clear line of sight to the target
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = target.GlobalPosition,
            CollisionMask = 1 // World collision layer
        };

        var result = spaceState.IntersectRay(query);

        // If no collision or the collision is with the target, return true
        return result.Count == 0 || result["collider"].As<Node>() == target;
    }

    protected virtual void HandleMovement()
    {
        if (!(CurrentTarget?.IsDead ?? true) && CurrentState == EnemyState.Chase)
            // Update the target position
            NavigationAgent.TargetPosition = CurrentTarget.GlobalPosition;

        if (!NavigationAgent.IsNavigationFinished())
        {
            var nextPathPosition = NavigationAgent.GetNextPathPosition();
            var direction = (nextPathPosition - GlobalPosition).Normalized();

            Velocity = direction * (float)StatsManager.MoveSpeed;
            MoveAndSlide();

            // Update action manager with current movement direction
            var actionManager = (EnemyActionManager)ActionManager;
            actionManager.SetMoveDirection(direction);
        }
        else
        {
            // Stop moving if we reached the destination
            Velocity = Vector2.Zero;
            var actionManager = (EnemyActionManager)ActionManager;
            actionManager.SetMoveDirection(Vector2.Zero);
        }
    }

    protected virtual void UpdateLookDirection()
    {
        if (CurrentTarget?.IsDead ?? true) return;

        var direction = (CurrentTarget.GlobalPosition - GlobalPosition).Normalized();
        var actionManager = (EnemyActionManager)ActionManager;
        actionManager.SetLookDirection(direction);

        // Optionally flip the sprite based on direction
        var isLookingRight = direction.X < 0;
        if (Sprite.FlipH != isLookingRight) Sprite.FlipH = isLookingRight;
    }

    public virtual void PerformAttack()
    {
        if (CurrentState == EnemyState.Attack) ActivateAbility(AbilitySlotType.Normal);
    }

    public override void TakeDamage(double amount, DamageType damageType, Vector2 knockback = new())
    {
        // Update health
        StatsManager.Health -= damageType == DamageType.Healing ? -amount : amount;

        // Emit event for damage taken
        EventBus.Publish(EventVariant.EntityStatChanged);

        // Check if entity died
        if (StatsManager.CurrentStats.Health <= 0)
        {
            IsDead = true;
            MoveableCounter = 0;

            // Publish death events
            EventBus.Publish(EventVariant.EntityDied);
            GlobalEventBus.Instance.Publish<BaseEntity>(GlobalEventVariant.EntityDied, this);

            // Handle death (can be extended in subclasses)
            Die();
        }

        // Display damage indicator
        Indicator.Spawn(this, amount, damageType);

        // Apply knockback
        Velocity += knockback;
        MoveAndSlide();
    }

    protected virtual void Die()
    {
        // Record the enemy defeat in game stats
        GameStats.Instance.RecordEnemyDefeat();
        
        // Drop currency based on chance
        if (GD.Randf() <= CurrencyDropChance) DropCurrency();
        
        // Base implementation just removes the entity
        QueueFree();
    }

    /// <summary>
    ///     Drops currency when enemy is defeated
    /// </summary>
    protected virtual void DropCurrency()
    {
        // Skip if no scene is assigned
        if (ChronoshardScene == null)
        {
            // Try to load the default scene
            ChronoshardScene = GD.Load<PackedScene>("res://Scenes/items/chronoshard.tscn");
            if (ChronoshardScene == null) return;
        }

        // Instantiate the chronoshard
        var chronoshard = ChronoshardScene.Instantiate<ChronoshardCollectible>();

        // Set random value within range
        chronoshard.Value = GD.RandRange(MinCurrencyDrop, MaxCurrencyDrop);

        // Position at enemy's location
        chronoshard.GlobalPosition = GlobalPosition;

        // Add small random offset
        chronoshard.GlobalPosition += new Vector2(
            GD.Randf() * 20 - 10,
            GD.Randf() * 20 - 10
        );

        // Add to the scene
        Callable.From(() => GetTree().Root.AddChild(chronoshard)).CallDeferred();

        // Publish event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.CurrencyDropped, chronoshard.Value);
    }

    // Ability implementation
    public override void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        AbilityManager.SetAbility(slot, ability);
    }

    public override void RemoveAbility(AbilitySlotType slot)
    {
        AbilityManager.RemoveAbility(slot);
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

    // Effect implementation
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
}
