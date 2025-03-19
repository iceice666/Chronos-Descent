using Godot;

namespace ChronosDescent.Scripts.Core.Ability.Variant;

public class Dash : BaseActiveAbility
{
    public Dash()
    {
        Id = "dash";
        Description = "Quickly dash in a direction";
        Cooldown = 3.0;
    }

    private static double DashDistance => 200.0;
    private static double DashSpeed => 1000.0;

    private bool _isDashing;
    private Vector2 _dashTarget;
    private Vector2 _dashDirection;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        _dashDirection = Caster.ActionManager.LookDirection.Normalized();

        _isDashing = true;
        _dashTarget = Caster.Position + _dashDirection * (float)DashDistance;
        
        Caster.Collision = false;
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        
        if (!_isDashing) return;
        
        var moveDistance = DashSpeed * delta;
        var distanceToTarget =  Caster.Position.DistanceTo(_dashTarget);
        
        
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
        
        _isDashing = false;
        Caster.Collision =true;
    }
}