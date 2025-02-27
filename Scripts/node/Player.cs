using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : CharacterBody2D
{
    public int MoveSpeed = 300;
    public int MaxMoveSpeed = 100;

    // For `onready`
    private AnimatedSprite2D _sprite;


    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }


    public override void _Process(double delta)
    {
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

        Velocity = direction * MoveSpeed;

        MoveAndSlide();
    }
}