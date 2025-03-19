using Godot;

namespace ChronosDescent.Scripts.Core.Ability.Variant;

public class Dash : BaseActiveAbility
{
    private Vector2 _dashDirection;
    private Vector2 _dashTarget;

    private bool _isDashing;

    public Dash()
    {
        Description = "Quickly dash in a direction";
       
    }

    private static double DashDistance => 200.0;
    private static double DashSpeed => 1000.0;

    public override string Id { get; protected set; } = "dash";
    public override double Cooldown { get; protected init; } = 3.0;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        _dashDirection = Caster.ActionManager.LookDirection.Normalized();

        _isDashing = true;
        _dashTarget = Caster.GlobalPosition + _dashDirection * (float)DashDistance;

        Caster.Collision = false;
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        if (!_isDashing) return;

        var moveDistance = DashSpeed * delta;
        var distanceToTarget = Caster.GlobalPosition.DistanceTo(_dashTarget);


        if (distanceToTarget <= moveDistance)
        {
            // We've reached the target
            Caster.GlobalPosition = _dashTarget;
            FinishDash();
        }
        else
        {
            // Move towards the target
            Caster.GlobalPosition += _dashDirection * (float)moveDistance;
        }
    }

    private void FinishDash()
    {
        if (Caster == null) return;

        _isDashing = false;
        Caster.Collision = true;
    }
}