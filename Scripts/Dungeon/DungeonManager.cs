using System;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Dungeon;

[GlobalClass]
public partial class DungeonManager : Node
{
    private Player _player;
    public static DungeonManager Instance { get; private set; }

    [Export] public RoomRegistryResource RoomRegistry { get; set; }
    public int Level { get; private set; }
    private DungeonNode DungeonMap { get; set; }

    private Control LoadingScreen { get; set; }
    private Node2D CurrentRoom { get; set; }

    public override void _Ready()
    {
        Instance = this;

        LoadingScreen = GetNode<Control>("../UI/Loading");
        _player = GetNode<Player>("../Player");


        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomCleared, () =>
        {
            CurrentRoom.GetNodeOrNull("Doors")?
                .GetChildren().Cast<RoomDoor>().ToList()
                .ForEach(door => door.Open());
        });

        MoveToNextLevel();
    }

    public void MoveToNextLevel()
    {
        Level++;
        DungeonMap = DungeonGenerator.Generate(Level);

        PrepareDungeonRoom();

        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            $"Level {Level}"
        );
    }

    public void MoveToNextRoom(bool isLeft)
    {
        if (DungeonMap.Type is RoomType.BossRoom or RoomType.EndRoom)
        {
            MoveToNextLevel();
            return;
        }

        DungeonMap = DungeonMap.NextNodes.Count == 1 || isLeft ? DungeonMap.NextNodes[0] : DungeonMap.NextNodes[1];

        PrepareDungeonRoom();
    }

    private async void PrepareDungeonRoom()
    {
        LoadingScreen.Visible = true;
        _player.Moveable = false;

        GetChildren().ToList().ForEach(node => node.QueueFree());

        var roomScene = (DungeonMap.Type switch
        {
            RoomType.StartRoom => RoomRegistry.StartRooms,
            RoomType.EndRoom => RoomRegistry.EndRooms,
            RoomType.BossRoom => RoomRegistry.BossRooms,
            RoomType.MiniBossRoom => RoomRegistry.MiniBossRooms,
            RoomType.CombatRoom => RoomRegistry.CombatRooms,
            RoomType.RewardRoom => RoomRegistry.RewardRooms,
            RoomType.EventRoom => RoomRegistry.EventRooms,
            RoomType.ShopRoom => RoomRegistry.ShopRooms,
            _ => throw new ArgumentOutOfRangeException()
        }).PickRandom();

        CurrentRoom = roomScene.Instantiate<Node2D>();


        Callable.From(() =>
        {
            AddChild(CurrentRoom);
            CurrentRoom.GetNodeOrNull("Doors")?
                .GetChildren().Cast<RoomDoor>().ToList()
                .ForEach(door => door.Close());
        }).CallDeferred();


        _player.GlobalPosition = CurrentRoom.GetNodeOrNull<Node2D>("EnterPoint")?.GlobalPosition ?? Vector2.Zero;

        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomEntered);


        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        _player.Moveable = true;
        LoadingScreen.Visible = false;
    }
}