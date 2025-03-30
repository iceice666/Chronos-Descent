using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Weapon;
using Godot;

namespace ChronosDescent.Scripts.Weapons;

public partial class BounceArrow : BaseProjectile
{
    private Hitbox _hitbox;
    private int _counter;
    private int _maxCounter = 3;
    private readonly HashSet<BaseEntity> _hitEntities = [];

    private int _speed;

    public void Initialize(
        BaseEntity attacker,
        Vector2 scale,
        Vector2 position,
        float rotation,
        Vector2 direction,
        int speed,
        float drag,
        double rawDamage
    )
    {
        Scale = scale;
        GlobalPosition = position;
        Rotation = rotation;
        Velocity = direction * speed;
        DragFactor = drag;
        _speed = speed;

        var hitbox = GetNode<Hitbox>("Hitbox");
        hitbox.RawDamage = rawDamage;
        hitbox.Attacker = attacker;
    }

    public override void _Ready()
    {
        _hitbox = GetNode<Hitbox>("Hitbox");

        _hitbox.BodyEntered += OnHit;
    }

    private void OnHit(Node2D body)
    {
        if (!body.IsInGroup("Entity"))
        {
            QueueFree();
            return;
        }

        if (_counter >= _maxCounter)
        {
            QueueFree();
            return;
        }

        _counter++;
        _hitEntities.Add((BaseEntity)body);

        var targets = _hitbox.GetNode("/root/BattleScene").GetTree().GetNodesInGroup("Entity")
            .Where(node =>
                !node.IsInGroup("Player")
                && node is BaseEntity e
                && !_hitEntities.Contains(e)
                && _hitbox.GlobalPosition.DistanceTo(e.GlobalPosition) < 100
            ).ToList();


        Node2D target;

        if (targets.Count == 0)
            target = (Node2D)_hitEntities.ElementAt((int)(GD.Randf() * _hitEntities.Count));
        else
            target = (Node2D)targets[(int)(GD.Randf() * targets.Count)];

        _hitEntities.Add((BaseEntity)body);

        var direction = (target.GlobalPosition - _hitbox.GlobalPosition).LimitLength();
        Rotation = direction.Angle();
        Velocity = direction * _speed;
    }
}
