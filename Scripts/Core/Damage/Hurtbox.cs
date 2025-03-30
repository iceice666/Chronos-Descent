using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Damage;

[GlobalClass]
public partial class Hurtbox : Area2D
{
    public new BaseEntity Owner { get; private set; }

    public override void _Ready()
    {
        Owner = GetOwner<BaseEntity>();
        CollisionLayer = 1 << 4;
    }
}