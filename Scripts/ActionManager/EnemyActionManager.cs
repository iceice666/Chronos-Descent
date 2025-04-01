using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Entities.Enemies;

public partial class EnemyActionManager : Node, IActionManager
{
    private BaseEnemy _owner;
    
    // IActionManager implementation
    public Vector2 MoveDirection { get; set; } = Vector2.Zero;
    public Vector2 LookDirection { get; set; } = Vector2.Zero;
    
    // Cooldowns and timers
    [Export] private double _attackCooldown = 1.0;
    private double _attackCooldownTimer = 0.0;
    
    public bool CanAttack => _attackCooldownTimer <= 0;
    
    public override void _Ready()
    {
        _owner = GetOwner<BaseEnemy>();
    }
    
    public override void _Process(double delta)
    {
        // Update cooldown timers
        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= delta;
        }
        
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