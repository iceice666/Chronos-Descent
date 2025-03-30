using System.Linq;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using ChronosDescent.Scripts.Effects;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public class ClaymoreGlobal
{
    public static bool NormalAnimationGoingForward { get; set; } = true;
}

public partial class Claymore : Node2D, IWeapon
{
    private Hitbox _hitbox;
    private BaseEntity _owner;

    [Export]
    public bool HitboxEnabled
    {
        get => _hitbox?.Enabled ?? false;
        set
        {
            if (_hitbox != null) _hitbox.Enabled = value;
        }
    }


    public BaseAbility NormalAttack { get; } = new NormalAbility();
    public BaseAbility SpecialAttack { get; } = new SpecialAbility();
    public BaseAbility Ultimate { get; } = new UltimateAbility();

    public void SetOwner(BaseEntity owner)
    {
        _owner = owner;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");
        _hitbox.RawDamage = 20;
        _hitbox.Attacker = _owner;


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
                _mountedWeapon._hitbox.Enabled = _animationTimer switch
                {
                    < 0.1 when _animationTimer + delta >= 0.1 => true,
                    < 0.2 when _animationTimer + delta >= 0.2 => false,
                    _ => _mountedWeapon._hitbox.Enabled
                };
            else
                _mountedWeapon._hitbox.Enabled = _animationTimer switch
                {
                    < 0.05 when _animationTimer + delta >= 0.05 => true,
                    < 0.1 when _animationTimer + delta >= 0.1 => false,
                    _ => _mountedWeapon._hitbox.Enabled
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
        private double _animationTimer;
        private Vector2 _targetPoint;


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
        }

        protected override void OnChannelingTick(double delta)
        {
            if (_animationTimer < 0.5)
            {
                if (_animationTimer + delta >= 0.5) DealExplodeDamage();

                Caster.GlobalPosition = Caster.GlobalPosition.Lerp(_targetPoint, (float)delta * 10.0f);
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

        private void DealExplodeDamage()
        {
            // Deal damage to entity within 50 units
            var entities = Caster.GetTree().GetNodesInGroup("Entity").Where(node =>
                node is BaseEntity e
                && e != Caster
                && Caster.GlobalPosition.DistanceSquaredTo(e.GlobalPosition) <= 50 * 50
            ).ToList();

            foreach (var entity in entities.Cast<BaseEntity>()) entity.TakeDamage(40, DamageType.Explosive);
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