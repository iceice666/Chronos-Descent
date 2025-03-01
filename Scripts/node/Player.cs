using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : Entity
{
    
    private UserInputManager _input;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");
        
        _input =  GetNode<UserInputManager>("UserInputManager");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        var direction = _input.GetMovementVector();

        var velocity = direction * (float)Stats.GetMoveSpeed();
        Animation.UpdateAnimation(velocity);

        Velocity = velocity;

        MoveAndSlide();
    }
}