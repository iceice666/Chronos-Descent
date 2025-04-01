using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts.Core.Damage;

[GlobalClass]
public partial class Hitbox : Area2D
{
    private CollisionPolygon2D _collisionObject;
    public BaseEntity Attacker;
    public EntityStats AttackerStats ;
    public double RawDamage;
    public int RawKnockback;
    public string ExcludedGroup;

    public bool Enabled
    {
        get => !_collisionObject.Disabled;
        set => _collisionObject.Disabled = !value;
    }

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 4;
        AreaEntered += OnEntityHit;
        _collisionObject = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnEntityHit;
    }

    private void OnEntityHit(Area2D area)
    {
        if (area.IsInGroup(ExcludedGroup) || area is not Hurtbox hurtbox) return;

        var attackee = hurtbox.Owner;
        if (attackee == Attacker) return;

        var kbDirection = (attackee.GlobalPosition - Attacker.GlobalPosition).Normalized() * RawKnockback;

        GlobalEventBus.Instance.Publish(GlobalEventVariant.DamageDealt,
            (RawDamage, attackee, AttackerStats ?? Attacker.StatsManager.CurrentStats, kbDirection));
    }
}