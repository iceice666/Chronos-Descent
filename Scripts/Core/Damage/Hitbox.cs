using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Scripts.Core.Damage;

[GlobalClass]
public partial class Hitbox : Area2D
{
    public IEntity Attackee;
    public EntityStats AttackerStats;
    public double RawDamage;

    public override void _Ready()
    {
        BodyEntered += OnEntityHit;
    }

    private void OnEntityHit(Node2D body)
    {
        if (!body.IsInGroup("Entity") || body is not IEntity entity) return;
        if (entity == Attackee) return;

        GlobalEventBus.Instance.Publish(EventBus.EventVariant.DamageDealt, (RawDamage, entity, AttackerStats));
    }
}