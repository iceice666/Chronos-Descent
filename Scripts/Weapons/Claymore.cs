using System.Linq;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
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
        get => _hitbox.Enabled;
        set => _hitbox.Enabled = value;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");
        _hitbox.RawDamage = 20;
        _hitbox.Attacker = _owner;


        Rotation = (float)Mathf.DegToRad(-114.0);
        Position = new Vector2(-6, 8);
    }


    public BaseAbility NormalAttack { get; } = new NormalAbility();
    public BaseAbility SpecialAttack { get; } = new SpecialAbility();
    public BaseAbility Ultimate { get; }

    public void SetOwner(BaseEntity owner)
    {
        _owner = owner;
    }


    public class NormalAbility : BaseChanneledAbility
    {
        public override string Id { get; protected set; } = "claymore_normal";
        public override double Cooldown { get; protected init; } = 0.3;
        public override double ChannelingDuration { get; protected set; } = 0.4;

        private double _animationTimer = 1;

        public override bool CanActivate() => CurrentCooldown <= 0;

        private Claymore _mountedWeapon;

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
            {
                _mountedWeapon._hitbox.Enabled = _animationTimer switch
                {
                    < 0.1 when _animationTimer + delta >= 0.1 => true,
                    < 0.2 when _animationTimer + delta >= 0.2 => false,
                    _ => _mountedWeapon._hitbox.Enabled
                };
            }
            else
            {
                _mountedWeapon._hitbox.Enabled = _animationTimer switch
                {
                    < 0.05 when _animationTimer + delta >= 0.05 => true,
                    < 0.1 when _animationTimer + delta >= 0.1 => false,
                    _ => _mountedWeapon._hitbox.Enabled
                };
            }

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
        private const float JumpDuration = 0.5f;
        private Vector2 _targetPoint;
        private double _animationTimer;


        public override string Id { get; protected set; } = "claymore_special";
        public override double Cooldown { get; protected init; } = 5;
        public override double ChannelingDuration { get; protected set; } = 0.8;
        public override bool CanActivate() => CurrentCooldown <= 0;


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
            // nop
        }

        private void DealExplodeDamage()
        {
            // Deal damage to entity within 50 units
            var entities = Caster.GetTree().GetNodesInGroup("Entity").Where(node =>
                node is BaseEntity e
                && e != Caster
                && Caster.GlobalPosition.DistanceSquaredTo(e.GlobalPosition) <= 50 * 50
            ).ToList();

            foreach (var entity in entities.Cast<BaseEntity>())
            {
                entity.TakeDamage(40, DamageType.Explosive);
            }
        }
    }
}