using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using Godot;

namespace ChronosDescent.Scripts.resource.Abilities.Example;

[GlobalClass]
public partial class DashAbility : Ability
{
    [Export] public double DashDistance { get; set; } = 200.0;
    [Export] public double DashSpeed { get; set; } = 1000.0;
    [Export] public bool DamageOnDash { get; set; }
    [Export] public double DashDamage { get; set; } = 15.0;
    [Export] public double DamageRadius { get; set; } = 50.0;

    private bool _isDashing;
    private Vector2 _dashDirection = Vector2.Zero;
    private Vector2 _dashTarget = Vector2.Zero;
    private bool _processing = true;

    public DashAbility()
    {
        Name = "Dash";
        Description = "Quickly dash in a direction";
        Type = AbilityType.Active;
        Cooldown = 3.0;
    }

    public override void Activate()
    {
        if (!CanActivate()) return;


        // Get the direction to dash
        Vector2 direction;
        if (Caster is Player player)
        {
            // For player, use the aim direction
            direction = player.GetNode<UserInputManager>("/root/Autoload/UserInputManager").AimInput;
            if (direction == Vector2.Zero)
            {
                direction = Vector2.Right * (player.GetNode<AnimationComponent>("AnimationComponent").FlipH ? -1 : 1);
            }
        }
        else
        {
            // For other entities, use velocity or default direction
            direction = Caster.Velocity.Normalized();
            if (direction == Vector2.Zero)
            {
                direction = Vector2.Right;
            }
        }

        // Set up the dash
        _isDashing = true;
        _dashDirection = direction.Normalized();
        _dashTarget = Caster.Position + _dashDirection * (float)DashDistance;

        // Disable movement control during dash
        Caster.Moveable = false;

        // Begin process for dash movement
        _processing = true;

        GD.Print($"{Caster.Name} activated {Name} in direction {_dashDirection}");
    }

    
    public override void Update(double delta)
    {
        UpdateState(delta);
        
        if (!_isDashing || Caster == null) return;

        // Calculate movement distance this frame
        var moveDistance = DashSpeed * delta;

        // Calculate direction to target
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

            // Check for damage on dash if enabled
            if (DamageOnDash)
            {
                DealDashDamage();
            }
        }
    }

    private void FinishDash()
    {
        if (Caster == null) return;

        // Deal final damage if enabled
        if (DamageOnDash)
        {
            DealDashDamage();
        }

        // Re-enable movement
        Caster.Moveable = true;

        // Clean up
        _isDashing = false;
        Caster = null;

        // Stop processing
        _processing = false;

        // Start cooldown
        StartCooldown();
    }

    private void DealDashDamage()
    {
        // Find entities in dash damage radius
        var targets = Caster.GetTree().GetNodesInGroup("Entity");

        foreach (var node in targets)
        {
            if (node is not Entity target || target == Caster) continue;

            var distance = Caster.GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distance <= DamageRadius)
            {
                target.TakeDamage(DashDamage);
            }
        }
    }

    // When ability is charged
    public override void ReleaseCharge()
    {
        if (!IsCharging) return;

        // Calculate charge percentage
        var chargePercentage = (CurrentChargeTime - MinChargeTime) / (MaxChargeTime - MinChargeTime);
        chargePercentage = Mathf.Clamp(chargePercentage, 0.0, 1.0);

        // Increase dash distance based on charge
        var originalDistance = DashDistance;
        DashDistance *= 1.0 + chargePercentage;

        // Activate the dash with increased distance
        Activate();

        // Reset distance for next time
        DashDistance = originalDistance;

        // Reset charging state
        IsCharging = false;
        CurrentChargeTime = 0.0;
    }
}