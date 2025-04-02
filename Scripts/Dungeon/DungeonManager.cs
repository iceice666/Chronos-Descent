using System;
using System.Linq;
using ChronosDescent.Scripts.Core;
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


        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.MagicButtonTriggered, OnMagicButtonPressed);

        MoveToNextLevel();
    }

    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.MagicButtonTriggered, OnMagicButtonPressed);

    }

    private void OnRoomCleared()
    {
        CurrentRoom.GetNodeOrNull("Doors")?
            .GetChildren().Cast<RoomDoor>().ToList()
            .ForEach(door => door.Open());
    }
    
    private void OnMagicButtonPressed(){
        while (true)
        {
            if (DungeonMap.Type is RoomType.BossRoom or RoomType.EndRoom)
            {
                Level++;
                DungeonMap = DungeonGenerator.Generate(Level);
            }

            if (DungeonMap.NextNodes[0].Type == RoomType.ShopRoom) break;

            DungeonMap = DungeonMap.NextNodes[0];
        }
    }

    public void MoveToNextLevel()
    {
        Level++;
        DungeonMap = DungeonGenerator.Generate(Level);
        
        // Update the level in GameStats
        GameStats.Instance.SetLevel(Level);

        PrepareDungeonRoom();

        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            $"Level {Level}"
        );
    }

    public void MoveToNextRoom(bool isLeft)
    {
        if (DungeonMap.Type is RoomType.BossRoom)
        {
            MoveToNextLevel();
            return;
        }

        DungeonMap = DungeonMap.NextNodes.Count == 1 || isLeft ? DungeonMap.NextNodes[0] : DungeonMap.NextNodes[1];

        if (DungeonMap.Type is RoomType.EndRoom)
        {
            MoveToNextLevel();
            return;
        }

        PrepareDungeonRoom();
    }

    private async void PrepareDungeonRoom()
    {
        LoadingScreen.Visible = true;
        _player.Moveable = false;

        GetChildren().ToList().ForEach(node => node.QueueFree());
        GetNode("/root/Autoload/Entities").GetChildren().ToList().ForEach(node => node.QueueFree());

        GD.Print($"Room type: {DungeonMap.Type}");

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

            if (DungeonMap.Type is not (RoomType.BossRoom or RoomType.MiniBossRoom or RoomType.CombatRoom))
                CurrentRoom.GetNodeOrNull("Doors")?
                    .GetChildren().Cast<RoomDoor>().ToList()
                    .ForEach(door => door.Open());
        }).CallDeferred();


        _player.GlobalPosition = CurrentRoom.GetNodeOrNull<Node2D>("EnterPoint")?.GlobalPosition ?? Vector2.Zero;

        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomStarted);

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        CurrentRoom.GetNodeOrNull<RoomController>("RoomController")?.OnRoomEntered();

        _player.Moveable = true;
        LoadingScreen.Visible = false;
    }
}