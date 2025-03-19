using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts;

public partial class Camera : Node2D
{
    private Vector2 _offset = new(0, 0);
    private IEntity _target;

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;
        // Smoothly move camera to follow target
        Position = Position.Lerp(_target.Position + _offset, (float)delta * 5.0f);
    }

    private void OnPlayerDead()
    {
        _target = null;
    }
}