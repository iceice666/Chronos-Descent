using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

[GlobalClass]
public partial class RoomDoor : Node2D
{
    public enum DoorType
    {
        Left,
        Right,
    }

    [Export] private DoorType _doorType;

    private AnimationPlayer _animationPlayer;
    private AnimatedSprite2D _animatedSprite;
    private Area2D _area2D;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _area2D = GetNode<Area2D>("Area2D");

        _area2D.BodyEntered += OnEntityEntered;
    }

    public override void _ExitTree()
    {
        _area2D.BodyEntered -= OnEntityEntered;
    }

    public void Open()
    {
        _animationPlayer.Play("open");
        _animatedSprite.Play("open");
    }

    public void Close()
    {
        _animationPlayer.PlayBackwards("open");
        _animatedSprite.PlayBackwards("open");
    }

    private void OnEntityEntered(Node2D node)
    {
        if (node is Player) DungeonManager.Instance.MoveToNextRoom(_doorType == DoorType.Left);
    }
}