using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : Entity
{
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");


        GetNode<Camera>("/root/Autoload/Camera").SwitchTarget(this);
        GetNode<UI.EffectsContainer>("/root/Autoload/UI/EffectsContainer").Initialize(this);
        GetNode<UI.AbilityContainer>("/root/Autoload/UI/AbilityContainer").Initialize(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Handle movements
        if (Moveable)
        {
            var direction = UserInputManager.Instance.MovementInput;
            var velocity = direction * (float)Stats.MoveSpeed;

            Velocity = velocity;
            Animation.UpdateWalkAnimation(velocity);

            MoveAndSlide();
        }
    }

    public override void _Process(double delta)
    {
        AimDirection = UserInputManager.Instance.AimInput;
        Animation.UpdateLookAnimation(AimDirection);
    }
}