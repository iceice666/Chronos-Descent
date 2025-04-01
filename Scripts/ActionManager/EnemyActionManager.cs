using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Entities.Enemies;
using Godot;

namespace ChronosDescent.Scripts.ActionManager;

[GlobalClass]
public partial class EnemyActionManager : Node, IActionManager
{
    // Cooldowns and timers
    [Export] private double _attackCooldown = 1.0;
    private double _attackCooldownTimer;
    private BaseEnemy _owner;

    public bool CanAttack => _attackCooldownTimer <= 0;

    // IActionManager implementation
    public Vector2 MoveDirection { get; set; } = Vector2.Zero;
    public Vector2 LookDirection { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        _owner = GetOwner<BaseEnemy>();
    }

    public override void _Process(double delta)
    {
        // Update cooldown timers
        if (_attackCooldownTimer > 0) _attackCooldownTimer -= delta;

        // Handle attack logic
        if (_owner.CurrentState == BaseEnemy.EnemyState.Attack && CanAttack)
        {
            _owner.PerformAttack();
            _attackCooldownTimer = _attackCooldown;
        }
    }

    // Methods to update movement and look direction
    public void SetMoveDirection(Vector2 direction)
    {
        MoveDirection = direction;
    }

    public void SetLookDirection(Vector2 direction)
    {
        LookDirection = direction;
    }
}