using System;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Effects;

[GlobalClass]
public sealed partial class Dissociation : BaseEffect
{
    public override string Id { get; protected set; } = "dissociation";

    public override string Description { get; protected set; } =
        "Create a separated snapshot, damage the snapshot received will return to the origin.";

    [Export] public override double Duration { get; set; }

    private DissociationSnapshot DissociatedEntity { get; set; }


    public override void OnApply()
    {
        var root = Owner.GetNode<Node2D>("..");
        DissociatedEntity = new DissociationSnapshot();
        root.AddChild(DissociatedEntity);

        var sprite = (Sprite2D)Owner.GetNodeOrNull("Sprite2D").Duplicate();
        var hurtBox = (CollisionShape2D)Owner.GetNodeOrNull("HurtBox").Duplicate();
        var takeDamageCb = Owner.TakeDamage;
        DissociatedEntity.Initialize(Owner);
    }

    public override void OnRemove()
    {
        DissociatedEntity?.QueueFree();
    }


    public partial class DissociationSnapshot : BaseEntity
    {
        private Action<double, DamageType, Vector2> _takeDamageCb;

        public override bool Collision { get; set; } = true;

        public void Initialize(
            BaseEntity origin
        )
        {
            AddChild((Sprite2D)origin.GetNodeOrNull("Sprite2D").Duplicate());
            CallDeferred(Node.MethodName.AddChild, (CollisionShape2D)origin.GetNodeOrNull("HurtBox").Duplicate());

            GlobalPosition = origin.GlobalPosition + new Vector2(GD.Randf() * 10f, GD.Randf() * 10f);
            Scale = origin.Scale;

            _takeDamageCb = origin.TakeDamage;
        }

        public override void TakeDamage(double amount, DamageType damageType, Vector2 knockback = new())
        {
            _takeDamageCb(amount, damageType, knockback);
        }

        public override void ApplyEffect(BaseEffect effect)
        {
            if (effect.Id == "dissociation") return;
            EffectManager.ApplyEffect(effect);
        }

        public override void RemoveEffect(string effectId)
        {
            EffectManager.RemoveEffect(effectId);
        }

        public override bool HasEffect(string effectId)
        {
            return EffectManager.HasEffect(effectId);
        }


        public override void SetAbility(AbilitySlotType slot, BaseAbility ability)
        {
            throw new NotImplementedException("Why are you here?");
        }

        public override void RemoveAbility(AbilitySlotType slot)
        {
            throw new NotImplementedException("Why are you here?");
        }

        public override void ActivateAbility(AbilitySlotType abilitySlotType)
        {
            throw new NotImplementedException("Why are you here?");
        }

        public override void ReleaseAbility(AbilitySlotType abilitySlotType)
        {
            throw new NotImplementedException("Why are you here?");
        }

        public override void CancelAbility(AbilitySlotType abilitySlotType)
        {
            throw new NotImplementedException("Why are you here?");
        }
    }
}