using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public class ClaymoreGlobal
{
   public static bool AttackCombo { get; set; } = true;
}

public partial class Claymore : Node2D, IWeapon
{
    private Hitbox _hitbox;

    [Export]
    public bool HitboxEnabled
    {
        get => _hitbox.Enabled;
        set => _hitbox.Enabled = value;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");
        Rotation = (float)Mathf.DegToRad(-114.0);
        Position = new Vector2(-6, 8);
    }

    public BaseAbility NormalAttack { get; } = new NormalAbility();
    public BaseAbility SpecialAttack { get; }
    public BaseAbility Ultimate { get; }
    
    
    
    public class NormalAbility : BaseChanneledAbility
    {
        public override string Id { get; protected set; } = "claymore_normal";
        public override double Cooldown { get; protected init; } = 0.5;
        public override double ChannelingDuration { get; protected set; } = 1.0;

        public override bool CanActivate() => CurrentCooldown <= 0;


        protected override void OnChannelingStart()
        {
            if (ClaymoreGlobal.AttackCombo)
                Caster.WeaponAnimationPlayer.Play($"claymore_normal");
            else
                Caster.WeaponAnimationPlayer.PlayBackwards("claymore_normal");
        }

        protected override void OnChannelingTick(double delta)
        {
        }

        protected override void OnChannelingComplete()
        {
            ClaymoreGlobal.AttackCombo = !ClaymoreGlobal.AttackCombo;
        }

        protected override void OnChannelingInterrupt()
        {
            Caster.WeaponAnimationPlayer.Stop();
        }
    }
}
