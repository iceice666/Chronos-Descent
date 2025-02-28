using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : Entity
{

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        var direction = new Vector2(
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
            Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")
        );

        if (direction.Length() > 1.0)
        {
            direction = direction.Normalized();
        }

        var velocity = direction * (float)Stats.GetMoveSpeed();
        Animation.UpdateAnimation(velocity);

        Velocity = velocity;

        MoveAndSlide();
    }
}