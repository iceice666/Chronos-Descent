using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts;

[GlobalClass]
public partial class Camera : Camera2D
{
    private Vector2 _offset = new(0, 0);
    private IEntity _target;

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;
        // Smoothly move camera to follow target
        Position = Position.Lerp(_target.GlobalPosition + _offset, (float)delta * 5.0f);
    }

    public override void _Ready()
    {
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.EntityDeath, (IEntity target) => OnEntityDead(target));
    }

    private void OnEntityDead(IEntity target)
    {
        if (_target != target) return;
        _target = null;
    }

    public void Initialize(IEntity target)
    {
        _target = target;
    }
}