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
    
    private CollisionPolygon2D _collisionObject;

    public bool Enabled
    {
        get => !_collisionObject.Disabled;
        set => _collisionObject.Disabled = !value;
    }

    public override void _Ready()
    {
        BodyEntered += OnEntityHit;
        _collisionObject = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
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