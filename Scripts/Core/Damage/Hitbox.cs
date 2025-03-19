using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts.Core.Damage;

[GlobalClass]
public partial class Hitbox : Area2D
{
    public IEntity Attacker;
    public EntityStats AttackerStats = null;
    public double RawDamage;

    public override void _Ready()
    {
        BodyEntered += OnEntityHit;
    }

    private void OnEntityHit(Node2D body)
    {
        if (!body.IsInGroup("Entity")) return;
        var attackee = (IEntity)body;
        if (attackee == Attacker) return;

        GlobalEventBus.Instance.Publish(GlobalEventVariant.DamageDealt,
            (RawDamage, attackee, AttackerStats ?? Attacker.StatsManager.CurrentStats));
    }
}