
using ChronosDescent.Scripts.Entity.Resource;
using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.Weapon;

[GlobalClass]
public partial class Hitbox : Area2D
{
    public Entity.Entity Caster;
    public BaseStats CasterStats;
    public double RawDamage;

    public override void _Ready()
    {
        BodyEntered += OnEntityHit;
    }


    private void OnEntityHit(Node2D body)
    {
        if (!body.IsInGroup("Entity")) return;
        var entity = body as Entity.Entity;
        if (entity == Caster) return;

        DamageManager.Instance.DealDamage(
            CasterStats == null ? (BaseStats)Caster.Stats.Current.Clone() : (BaseStats)CasterStats.Clone(),
            entity,
            RawDamage
        );
    }
}