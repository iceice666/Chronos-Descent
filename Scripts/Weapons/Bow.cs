using System;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public partial class Bow : Node2D, IWeapon
{
    public BaseAbility NormalAttack { get; private set; }
    public BaseAbility SpecialAttack { get; private set; }
    public BaseAbility Ultimate { get; private set; }

    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        var spawnPoint = GetNode<Node2D>("ArrowSpawnPoint");

        NormalAttack = new NormalAbility(spawnPoint, PlayAnimation);
        SpecialAttack = new SpecialAbility(spawnPoint, PlayAnimation);
        Ultimate = new UltimateAbility(spawnPoint, PlayAnimation);
    }

    private void PlayAnimation(string animation)
    {
        _animatedSprite.Play(animation);
    }
}

/// <summary>
/// Base class for all bow abilities, providing common arrow functionality
/// </summary>
public abstract class BowAbility(Node2D spawnPoint, Action<string> playAnimation) : BaseChargedAbility
{
    protected readonly Node2D SpawnPoint = spawnPoint;
    protected readonly Action<string> PlayAnimation = playAnimation;

    // Common configuration values
    protected const string ChargingAnimation = "charging";
    protected const string NormalAnimation = "normal";
    protected const float DefaultScale = 0.8f;
    protected const float DefaultDrag = 0.97f;
    
    public override double MinChargeTime { get; set; } = 0.2f;
    public override double MaxChargeTime { get; set; } = 0.4f;
    public override bool AutoCastWhenFull { get; set; } = false;

    public override bool CanActivate() => CurrentCooldown <= 0;

    public override void Activate()
    {
        base.Activate();
        PlayAnimation(ChargingAnimation);
    }

    protected override void OnChargingCanceled()
    {
        PlayAnimation(NormalAnimation);
    }

    /// <summary>
    /// Calculate damage based on charge time
    /// </summary>
    protected double CalculateDamage(double baseDamage)
    {
        // Scale damage between 80% and 150% based on charge time
        double chargePercentage = Math.Min(CurrentChargeTime / MinChargeTime, 2.0);
        return baseDamage * (0.8 + (0.7 * chargePercentage));
    }

    /// <summary>
    /// Get the projectiles spawn node
    /// </summary>
    protected Node GetProjectilesNode() => SpawnPoint.GetNode("/root/Autoload/Projectiles");
}

public class NormalAbility : BowAbility
{
    private readonly PackedScene _projectileScene = GD.Load<PackedScene>("res://Scenes/weapon/arrow.tscn");
    private const int Speed = 300;
    private const double BaseDamage = 10;

   
    public override string Id { get; protected set; } = "bow_normal";
    public override double Cooldown { get; protected init; } = 0.25;

    public NormalAbility(Node2D spawnPoint, Action<string> playAnimation)
        : base(spawnPoint, playAnimation)
    {
        Description = "Shoot an arrow that deals damage based on charge time";
    }

    public override void Execute()
    {
        var projectile = _projectileScene.Instantiate<Arrow>();
        var direction = Caster.ActionManager.LookDirection;

        GetProjectilesNode().AddChild(projectile);
        projectile.Initialize(
            Caster,
            new Vector2(DefaultScale, DefaultScale),
            SpawnPoint.GlobalPosition,
            direction.Angle(),
            direction * Speed,
            DefaultDrag,
            CalculateDamage(BaseDamage)
        );

        PlayAnimation(NormalAnimation);
    }
}

public class SpecialAbility : BowAbility
{
    private readonly PackedScene _projectileScene = GD.Load<PackedScene>("res://Scenes/weapon/arrow.tscn");
    private const int Speed = 300;
    private const double BaseDamage = 8;
    private const int ArrowCount = 5;
    private const float SpreadAngle = 15f; // Degrees

    public override double MinChargeTime { get; set; } = 0.4f;
    public override string Id { get; protected set; } = "bow_special";
    public override double Cooldown { get; protected init; } = 0.8;

    public SpecialAbility(Node2D spawnPoint, Action<string> playAnimation)
        : base(spawnPoint, playAnimation)
    {
        Description = "Fire multiple arrows in a spread pattern";
    }

    public override void Execute()
    {
        var spawnNode = GetProjectilesNode();
        var mountPoint = SpawnPoint.GlobalPosition;
        var mountPointOffset = SpawnPoint.Position;
        var originPoint = mountPoint - mountPointOffset;

        var radius = mountPointOffset.Length();
        var theta = mountPointOffset.Angle() + SpawnPoint.GlobalRotation;
        var damage = CalculateDamage(BaseDamage);

        // Create array of angles with even spread
        var halfSpread = Mathf.DegToRad(SpreadAngle);
        var angleStep = (2 * halfSpread) / (ArrowCount - 1);

        for (var i = 0; i < ArrowCount; i++)
        {
            var angle = theta - halfSpread + (angleStep * i);
            var angleVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            var projectile = _projectileScene.Instantiate<Arrow>();
            spawnNode.AddChild(projectile);

            projectile.Initialize(
                Caster,
                new Vector2(DefaultScale, DefaultScale),
                originPoint + radius * angleVector,
                angle,
                angleVector * Speed,
                DefaultDrag,
                damage
            );
        }

        PlayAnimation(NormalAnimation);
    }
}

public class UltimateAbility : BowAbility
{
    private readonly PackedScene _projectileScene = GD.Load<PackedScene>("res://Scenes/weapon/bounce_arrow.tscn");
    private const int Speed = 300;
    private const double BaseDamage = 30;

    public override double MaxChargeTime { get; set; } = 1.0;
    public override bool AutoCastWhenFull { get; set; } = true;
    public override string Id { get; protected set; } = "bow_ultimate";
    public override double Cooldown { get; protected init; } = 5.0;

    public UltimateAbility(Node2D spawnPoint, Action<string> playAnimation)
        : base(spawnPoint, playAnimation)
    {
        Description = "Fire a powerful arrow that bounces between targets";
    }

    public override void Execute()
    {
        var projectile = _projectileScene.Instantiate<BounceArrow>();
        var direction = Caster.ActionManager.LookDirection;

        GetProjectilesNode().AddChild(projectile);
        projectile.Initialize(
            Caster,
            new Vector2(DefaultScale, DefaultScale),
            SpawnPoint.GlobalPosition,
            direction.Angle(),
            direction,
            Speed,
            DefaultDrag,
            CalculateDamage(BaseDamage)
        );

        PlayAnimation(NormalAnimation);
    }
}