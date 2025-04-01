using System.Linq;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using ChronosDescent.Scripts.Effects;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public static class ClaymoreGlobal
{
    public static bool NormalAnimationGoingForward { get; set; } = true;
}

public partial class Claymore : Node2D, IWeapon
{
    public required Hitbox Hitbox;
    public required BaseEntity WeaponOwner;

    [Export]
    public bool HitboxEnabled
    {
        get => Hitbox.Enabled;
        set => Hitbox.Enabled = value;
    }


    public BaseAbility NormalAttack { get; } = new NormalAbility();
    public BaseAbility SpecialAttack { get; } = new SpecialAbility();
    public BaseAbility Ultimate { get; } = new UltimateAbility();

    public void SetOwner(BaseEntity owner)
    {
        WeaponOwner = owner;
    }

    public override void _Ready()
    {
        Hitbox = GetNode<Hitbox>("Hitbox");
        Hitbox.RawDamage = 20;
        Hitbox.Attacker = WeaponOwner;
        Hitbox.RawKnockback = 100;
        Hitbox.ExcludedGroup = "Player";


        Rotation = (float)Mathf.DegToRad(-114.0);
        Position = new Vector2(-6, 8);
    }


    public class NormalAbility : BaseChanneledAbility
    {
        private double _animationTimer = 1;

        private Claymore _mountedWeapon;
        public override string Id { get; protected set; } = "claymore_normal";
        public override double Cooldown { get; protected init; } = 0.3;
        public override double ChannelingDuration { get; protected set; } = 0.4;

        public override bool CanActivate()
        {
            return CurrentCooldown <= 0;
        }

        protected override void OnChannelingStart()
        {
            _animationTimer = 0;
            _mountedWeapon = (Claymore)Caster.WeaponManager.CurrentWeapon;

            Caster.WeaponAnimationPlayer.Play(ClaymoreGlobal.NormalAnimationGoingForward
                ? "claymore_normal_1"
                : "claymore_normal_2");
        }

        protected override void OnChannelingTick(double delta)
        {
            if (ClaymoreGlobal.NormalAnimationGoingForward)
                _mountedWeapon.Hitbox.Enabled = _animationTimer switch
                {
                    < 0.1 when _animationTimer + delta >= 0.1 => true,
                    < 0.2 when _animationTimer + delta >= 0.2 => false,
                    _ => _mountedWeapon.Hitbox.Enabled
                };
            else
                _mountedWeapon.Hitbox.Enabled = _animationTimer switch
                {
                    < 0.05 when _animationTimer + delta >= 0.05 => true,
                    < 0.1 when _animationTimer + delta >= 0.1 => false,
                    _ => _mountedWeapon.Hitbox.Enabled
                };

            _animationTimer += delta;
        }

        protected override void OnChannelingComplete()
        {
            ClaymoreGlobal.NormalAnimationGoingForward = !ClaymoreGlobal.NormalAnimationGoingForward;
        }

        protected override void OnChannelingInterrupt()
        {
            Caster.WeaponAnimationPlayer.Stop();
        }
    }

    public class SpecialAbility : BaseChanneledAbility
    {
        private const int JumpDistance = 100;
        private const float Speed = 200.0f; // Adjust this speed as needed
        private const float Acceleration = 1000.0f; // How quickly it reaches target speed
        private double _animationTimer;
        private Vector2 _targetPoint;
        private Vector2 _velocity;

        public override string Id { get; protected set; } = "claymore_special";
        public override double Cooldown { get; protected init; } = 5;
        public override double ChannelingDuration { get; protected set; } = 0.8;

        public override bool CanActivate()
        {
            return CurrentCooldown <= 0;
        }

        protected override void OnChannelingStart()
        {
            _targetPoint = Caster.GlobalPosition + Caster.ActionManager.LookDirection * JumpDistance;

            if (ClaymoreGlobal.NormalAnimationGoingForward)
                Caster.WeaponAnimationPlayer.Play(
                    ClaymoreGlobal.NormalAnimationGoingForward
                        ? "claymore_special_1"
                        : "claymore_special_2"
                );

            _animationTimer = 0;
            _velocity = Vector2.Zero; // Reset velocity
        }

        protected override void OnChannelingTick(double delta)
        {
            if (_animationTimer < 0.5)
            {
                if (_animationTimer + delta >= 0.5)
                {
                    DealExplodeDamage();
                    _velocity = Vector2.Zero; // Stop movement when explosion happens
                }

                // Calculate direction and desired velocity
                var direction = (_targetPoint - Caster.GlobalPosition).Normalized();
                var targetVelocity = direction * Speed;

                // Smoothly adjust velocity
                _velocity = _velocity.MoveToward(targetVelocity, Acceleration * (float)delta);

                // Apply velocity to CharacterBody2D
                Caster.Velocity = _velocity;
                Caster.MoveAndSlide();

                // Check if we've reached the target
                var distance = Caster.GlobalPosition.DistanceTo(_targetPoint);
                if (distance < 5.0f) // Small threshold to consider "arrived"
                {
                    _velocity = Vector2.Zero;
                    Caster.Velocity = _velocity;
                }
            }

            _animationTimer += delta;
        }

        protected override void OnChannelingComplete()
        {
            // Ensure we stop moving when complete
            Caster.Velocity = Vector2.Zero;
        }

        protected override void OnChannelingInterrupt()
        {
            Caster.WeaponAnimationPlayer.Stop();
            Caster.Velocity = Vector2.Zero;
        }

        private void DealExplodeDamage()
        {
            var entities = Caster.GetTree().GetNodesInGroup("Entity").Where(node =>
                node is BaseEntity e
                && e != Caster
                && Caster.GlobalPosition.DistanceSquaredTo(e.GlobalPosition) <= 50 * 50
            ).ToList();

            foreach (var entity in entities.Cast<BaseEntity>())
                entity.TakeDamage(40, DamageType.Explosive);
        }
    }

    public class UltimateAbility : BaseChanneledAbility
    {
        private const int DamageRadius = 120;
        private const float TimeSlowDuration = 5.0f;

        private double _animationTimer;
        private Claymore _mountedWeapon;

        public override string Id { get; protected set; } = "claymore_ultimate";
        public override double Cooldown { get; protected init; } = 25.0;
        public override double ChannelingDuration { get; protected set; } = 2;

        public override bool CanActivate()
        {
            return CurrentCooldown <= 0;
        }

        protected override void OnChannelingStart()
        {
            _animationTimer = 0;
            _mountedWeapon = (Claymore)Caster.WeaponManager.CurrentWeapon;

            // Play wind-up animation
            Caster.WeaponAnimationPlayer.Play("claymore_ultimate_windup");
        }

        protected override void OnChannelingTick(double delta)
        {
            switch (_animationTimer)
            {
                case < 1.0 when _animationTimer + delta >= 1.0:
                    Caster.WeaponAnimationPlayer.Play("claymore_ultimate_strike");
                    break;
                case < 1.8 when _animationTimer + delta >= 1.8:
                    DealTimeCleaveDamage();
                    break;
            }

            _animationTimer += delta;
        }

        protected override void OnChannelingComplete()
        {
        }

        protected override void OnChannelingInterrupt()
        {
            Caster.WeaponAnimationPlayer.Stop();
        }

        private void DealTimeCleaveDamage()
        {
            // Calculate cone direction based on player's facing direction
            var coneDirection = Caster.ActionManager.LookDirection.Normalized();

            // Find entities in a radius
            var entities = Caster.GetTree().GetNodesInGroup("Entity").Where(node =>
                node is BaseEntity e
                && e != Caster
                && Caster.GlobalPosition.DistanceSquaredTo(e.GlobalPosition) <= DamageRadius * DamageRadius
            ).Cast<BaseEntity>().ToList();

            foreach (var entity in entities)
            {
                // Calculate angle between player's direction and direction to entity
                var directionToEntity = (entity.GlobalPosition - Caster.GlobalPosition).Normalized();
                var angle = Mathf.Abs(coneDirection.AngleTo(directionToEntity));

                // Check if entity is within a 90-degree cone (π/2 radians)
                if (angle <= Mathf.Pi / 2)
                {
                    // Calculate damage falloff based on distance
                    var distanceRatio = 1.0f - Caster.GlobalPosition.DistanceTo(entity.GlobalPosition) / DamageRadius;
                    var damageMultiplier = Mathf.Max(0.5f, distanceRatio);

                    // Deal damage
                    var damage = (int)(80 * damageMultiplier);
                    entity.TakeDamage(damage, DamageType.Normal);

                    // Apply slow effect
                    entity.EffectManager.ApplyEffect(new Slow(TimeSlowDuration));
                }
            }

            // Visual effects for the time cleave would go here
            // Could use particle effects, shader effects, etc.
        }
    }
}