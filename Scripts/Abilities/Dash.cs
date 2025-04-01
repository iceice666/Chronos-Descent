using ChronosDescent.Scripts.Core.Ability;
using Godot;

namespace ChronosDescent.Scripts.Abilities;

public class Dash : BaseChanneledAbility
{
    private const int JumpDistance = 100;
    private const float Speed = 200.0f; // Adjust this speed as needed
    private const float Acceleration = 1000.0f; // How quickly it reaches target speed
    private Vector2 _targetPoint;
    private Vector2 _velocity;

    public Dash()
    {
        Description = "Quickly dash in a direction";
    }

    public override string Id { get; protected set; } = "dash";
    public override double Cooldown { get; protected init; } = 1;
    public override double ChannelingDuration { get; protected set; } = 0.5;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    protected override void OnChannelingStart()
    {
        _targetPoint = Caster.GlobalPosition + Caster.ActionManager.LookDirection * JumpDistance;
        _velocity = Vector2.Zero; // Reset velocity
    }

    protected override void OnChannelingTick(double delta)
    {
        // Calculate direction and desired velocity
        var direction = (_targetPoint - Caster.GlobalPosition).Normalized();
        var targetVelocity = direction * Speed;

        // Smoothly adjust velocity
        _velocity = _velocity.MoveToward(targetVelocity, Acceleration * (float)delta);

        // Apply velocity to CharacterBody2D
        Caster.Velocity = _velocity;
        Caster.MoveAndSlide();

        // Check if we've reached the target
        var distance = Caster.GlobalPosition.DistanceTo(_targetPoint);
        if (distance < 5.0f) // Small threshold to consider "arrived"
        {
            _velocity = Vector2.Zero;
            Caster.Velocity = _velocity;
        }
    }

    protected override void OnChannelingComplete()
    {
        // Ensure we stop moving when complete
        Caster.Velocity = Vector2.Zero;
    }

    protected override void OnChannelingInterrupt()
    {
        Caster.Velocity = Vector2.Zero;
    }
}