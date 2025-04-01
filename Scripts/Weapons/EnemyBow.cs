using System;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public partial class EnemyBow : Node2D, IWeapon
{
    private AnimatedSprite2D _animatedSprite;
    private BaseEntity _owner;
    private Node2D _spawnPoint;

    public BaseAbility NormalAttack { get; private set; }
    public BaseAbility SpecialAttack { get; } = null; // No special attack for enemy bow
    public BaseAbility Ultimate { get; } = null; // No ultimate for enemy bow

    public void SetOwner(BaseEntity owner)
    {
        _owner = owner;
    }

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _spawnPoint = GetNode<Node2D>("ArrowSpawnPoint");

        NormalAttack = new ShootAbility(_spawnPoint, PlayAnimation);

        AddToGroup("NeedRotation");
    }

    private void PlayAnimation(string animation)
    {
        _animatedSprite.Play(animation);
    }

    public class ShootAbility : BaseActiveAbility
    {
        private const int Speed = 250;
        private const double BaseDamage = 8;
        private const float DefaultScale = 0.8f;
        private const float DefaultDrag = 0.95f;
        private readonly Action<string> _playAnimation;
        private readonly PackedScene _projectileScene = GD.Load<PackedScene>("res://Scenes/weapon/arrow.tscn");

        private readonly Node2D _spawnPoint;

        public ShootAbility(Node2D spawnPoint, Action<string> playAnimation)
        {
            _spawnPoint = spawnPoint;
            _playAnimation = playAnimation;
            Description = "Enemy bow attack";
        }

        public override string Id { get; protected set; } = "enemy_bow_shoot";
        public override double Cooldown { get; protected init; } = 2.0; // Longer cooldown for enemies

        public override bool CanActivate()
        {
            return CurrentCooldown <= 0;
        }

        public override void Execute()
        {
            // Get target direction from the caster's action manager
            var direction = Caster.ActionManager.LookDirection;

            // Play animation
            _playAnimation("charging");

            // Get projectiles node
            var projectilesNode = _spawnPoint.GetNode("/root/Autoload/Projectiles");

            // Instantiate arrow
            var projectile = _projectileScene.Instantiate<Arrow>();
            projectilesNode.AddChild(projectile);

            // Initialize arrow
            projectile.Initialize(
                Caster,
                new Vector2(DefaultScale, DefaultScale),
                _spawnPoint.GlobalPosition,
                direction.Angle(),
                direction * Speed,
                DefaultDrag,
                BaseDamage,
                "Enemy"
            );
        }
    }
}