using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : Entity
{
    
    private UserInputManager _input;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");
        
        _input =  GetNode<UserInputManager>("/root/Autoload/UserInputManager");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        var direction = _input.MovementInput;
        var velocity = direction * (float)Stats.GetMoveSpeed();
        
        Velocity = velocity;
        Animation. UpdateWalkAnimation(velocity);

        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        var look = _input.AimInput;
        Animation.UpdateLookAnimation(look);
    }
}