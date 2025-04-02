using ChronosDescent.Scripts.Core.Ability;
using Godot;

namespace ChronosDescent.Scripts.Abilities;

public partial class Dash : BaseActiveAbility
{
    private Vector2 _dashDirection = Vector2.Zero;
    private Vector2 _dashTarget = Vector2.Zero;
    private bool _isDashing;

    public override string Id { get; protected set; } = "Dash";

    public override double Cooldown { get; protected init; } = 3.0;

    public Dash()
    {
        Description = "Quickly dash in a direction";
    }

    [Export] public double DashDistance { get; set; } = 200.0;
    [Export] public double DashSpeed { get; set; } = 1000.0;

    public override void Execute()
    {
        // Get the direction to dash
        Vector2 direction = Caster.ActionManager.LookDirection;
        if (direction == Vector2.Zero) return;

        // Set up the dash
        _isDashing = true;
        _dashDirection = direction.Normalized();
        _dashTarget = Caster.Position + _dashDirection * (float)DashDistance;


        // Disable collision
        Caster.Collision = false;
        Caster.Moveable = false;
    }


    public override bool CanActivate() => CurrentCooldown <= 0;


    public override void Update(double delta)
    {
        base.Update(delta);

        if (!_isDashing) return;

        // Calculate movement distance this frame
        var moveDistance = DashSpeed * delta;

        // Calculate the direction to target
        var currentPosition = Caster.Position;
        var distanceToTarget = currentPosition.DistanceTo(_dashTarget);

        if (distanceToTarget <= moveDistance)
        {
            // We've reached the target
            Caster.Position = _dashTarget;
            FinishDash();
        }
        else
        {
            // Move towards the target
            Caster.Position += _dashDirection * (float)moveDistance;
        }
    }


    private void FinishDash()
    {
        if (Caster == null) return;

        // Re-enable movement
        Caster.Moveable = true;

        // Clean up
        _isDashing = false;

        // Enable collision
        Caster.Collision = true;
    }
}