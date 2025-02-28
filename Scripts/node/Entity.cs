using System;
using Godot;

namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class Entity : CharacterBody2D
{
    private double _health = 100;
    private double _maxHealth = 100;

    private double _mana = 100;
    private double _maxMana = 100;

    private double _defense = 100;

    private double _strength = 10;
    private double _intelligence = 10;

    private double _criticalChance = 50;
    private double _criticalDamage = 100;

    private double _attackSpeed = 4;

    private double _moveSpeed = 300;

    protected double MoveSpeed
    {
        get => _moveSpeed;
        private set => _moveSpeed = Math.Min(value, MaxMoveSpeed);
    }

    private const double MaxMoveSpeed = 1000;

    // For `onready`
    private AnimatedSprite2D _sprite;


    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }


    public override void _Process(double delta)
    {
        if (Velocity == Vector2.Zero)
        {
            _sprite.Animation = "idle";
        }
        else if (Velocity.X < 0)
        {
            _sprite.FlipH = true;
            _sprite.Animation = "walk";
        }
        else if (Velocity.X > 0)
        {
            _sprite.FlipH = false;
            _sprite.Animation = "walk";
        }
        else if (Velocity.Y != 0)
        {
            _sprite.Animation = "walk";
        }
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    public void TakeDamage(double amount)
    {
        _health -= amount;
        _sprite.Animation = "hurt";

        if (_health <= 0)
        {
            OnEntityDeath();
        }

        // Damage indicator
    }

    public void OnEntityDeath()
    {
        _sprite.Animation = "death";
        QueueFree();
    }
}