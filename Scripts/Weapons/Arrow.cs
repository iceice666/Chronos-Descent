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
        double rawDamage
    )
    {
        Scale = scale;
        GlobalPosition = position;
        Rotation = rotation;
        Velocity = velocity;
        DragFactor = drag;

        var hitbox = GetNode<Hitbox>("Hitbox");
        hitbox.RawDamage = rawDamage;
        hitbox.Attacker = attacker;
        hitbox.RawKnockback = 10;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");

        _hitbox.BodyEntered += _ => { QueueFree(); };
    }
}