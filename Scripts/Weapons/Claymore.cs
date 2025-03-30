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
    public BaseAbility SpecialAttack { get; }
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

        private double _animationTimer;

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
}