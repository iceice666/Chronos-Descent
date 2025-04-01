using System.Linq;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public partial class EnemyClaymore : Node2D, IWeapon
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
    public BaseAbility SpecialAttack { get; } = null; // No special attack for enemies
    public BaseAbility Ultimate { get; } = null; // No ultimate for enemies

    public void SetOwner(BaseEntity owner)
    {
        _owner = owner;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");
        _hitbox.RawDamage = 20;
        _hitbox.Attacker = _owner;
        _hitbox.RawKnockback = 100;

        Rotation = (float)Mathf.DegToRad(-114.0);
        Position = new Vector2(-6, 8);
    }

    public class NormalAbility : BaseChanneledAbility
    {
        private double _animationTimer = 1;

        private EnemyClaymore _mountedWeapon;
        public override string Id { get; protected set; } = "enemy_claymore_normal";
        public override double Cooldown { get; protected init; } = 1.0; // 1 second delay between attacks
        public override double ChannelingDuration { get; protected set; } = 0.4;

        public override bool CanActivate()
        {
            return CurrentCooldown <= 0;
        }

        protected override void OnChannelingStart()
        {
            _animationTimer = 0;
            _mountedWeapon = (EnemyClaymore)Caster.WeaponManager.CurrentWeapon;

            // Use the same animations from the Claymore weapon
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
}