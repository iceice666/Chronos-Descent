using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public partial class Arrow : BaseProjectile
{
    private Hitbox _hitbox;

    public void Initialize(
        BaseEntity attacker,
        Vector2 scale,
        Vector2 position,
        float rotation,
        Vector2 velocity,
        float drag,
        double rawDamage,
        string shooterGroup
    )
    {
        Scale = scale;
        GlobalPosition = position;
        Rotation = rotation;
        Velocity = velocity;
        DragFactor = drag;

        _hitbox.RawDamage = rawDamage;
        _hitbox.Attacker = attacker;
        _hitbox.RawKnockback = 10;
        _hitbox.ExcludedGroup = shooterGroup;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");
        _hitbox.BodyEntered += _ => { QueueFree(); };
    }
}