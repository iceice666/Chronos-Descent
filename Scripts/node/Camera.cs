using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Camera : Node2D
{
    private Vector2 _offset = new(0, 0);
    private Entity _target;

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


    public void SwitchTarget(Entity target)
    {
        _target = target;
        _target.Combat.Death += OnPlayerDead;
        GD.Print($"Camera: Switch Target: {_target}");
    }

    public override void _ExitTree()
    {
        if (_target != null) _target.Combat.Death -= OnPlayerDead;
    }
}