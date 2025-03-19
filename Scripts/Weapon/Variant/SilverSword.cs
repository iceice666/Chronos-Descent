using ChronosDescent.Scripts.Ability;
using ChronosDescent.Scripts.Ability.Node;
using ChronosDescent.Scripts.Entity.Resource;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Weapon.Variant;

public partial class SilverSword : BaseWeapon
{
    private AnimationPlayer _animation;
    private Hitbox _hitbox;

    private bool _prevXLessThanZero = true;

    private AnimatedSprite2D _weapon;

    public SilverSword()
    {
        Id = "silver_sword";
        Description = "A normal Silver Sword";
        Icon = GD.Load<Texture2D>("res://Assets/Weapon/silver_sword.png");

        NormalAttack = new AttackAbility();
        SpecialAttack = new SpecialAbility();
    }

    public override void _Ready()
    {
        _weapon = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _weapon.Rotation = -1.05f;
        _weapon.Position = new Vector2(6, -7);

        _hitbox = _weapon.GetNode<Hitbox>("Hitbox");
        _hitbox.RawDamage = 10;
        _hitbox.Caster = (Entity.Entity)GetParent().Owner;
    }

    public override void UpdateLookAnimation(Vector2 vec)
    {
        switch (vec.X)
        {
            case > 0 when _prevXLessThanZero:
                Scale = Vector2.One;
                _prevXLessThanZero = false;

                break;

            case < 0 when !_prevXLessThanZero:
                Scale = new Vector2(-1, 1);
                _prevXLessThanZero = true;

                break;
        }
    }


    public partial class AttackAbility : BaseActiveAbility
    {
        public AttackAbility()
        {
            Id = "silver_sword_normal";
            RequiredSlot = AbilitySlot.WeaponAttack;
            Cooldown = 0.9;
        }

        public override void Activate()
        {
            if (!CanActivate()) return;


            var cooldownMultiplier = Caster.Stats.AttackSpeed / Caster.Stats.Base.AttackSpeed;


            ExecuteEffect(cooldownMultiplier);
            StartCooldown(1 / cooldownMultiplier);


            OnStateChanged(new AbilityStateEventArgs(this, AbilityState.Cooldown));
        }

        private void ExecuteEffect(double attackSpeed)
        {
            Caster.Weapon.Animation.Play(RequiredSlot.GetSlotName(), customSpeed: (float)attackSpeed);
        }
    }


    public partial class SpecialAbility : BaseActiveAbility
    {
        public SpecialAbility()
        {
            Id = "silver_sword_special";
            RequiredSlot = AbilitySlot.WeaponSpecial;
            Cooldown = 10.0f;
        }

        protected override void ExecuteEffect()
        {
            Caster.ApplyEffect(new AbilityEffect());
        }

        public sealed partial class AbilityEffect : Effect.Effect
        {
            public AbilityEffect()
            {
                Identifier = "silver_sword_special_effect";
                Name = "Silver Sword Special Effect";
                Description = "Increase move speed and attack speed";
                Duration = 7.0;
                Behaviors = EffectBehavior.StatModifier;
                IsStackable = false;

                AdditiveModifiers = new Dictionary<BaseStats.Specifier, double>
                {
                    {
                        BaseStats.Specifier.MoveSpeed, 30
                    },
                    {
                        BaseStats.Specifier.AttackSpeed, 10
                    }
                };
            }
        }
    }
}