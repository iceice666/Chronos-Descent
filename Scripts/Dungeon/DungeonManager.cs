using System;
using System.Linq;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Dungeon.Generation;
using ChronosDescent.Scripts.Dungeon.Room;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Dungeon;

[GlobalClass]
public partial class DungeonManager : Node
{
    private readonly DungeonGenerator _dungeonGenerator = new();
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
        if (DungeonMap.Type is RoomType.EndRoom)
        {
            MoveToNextLevel();
            return;
        }

        CurrentRoom.GetNodeOrNull("Doors")?
            .GetChildren().Cast<RoomDoor>().ToList()
            .ForEach(door => door.Open());
    }

    private void OnMagicButtonPressed()
    {
        while (true)
        {
            if (DungeonMap.Type is RoomType.BossRoom or RoomType.EndRoom)
            {
                Level++;
                DungeonMap = _dungeonGenerator.Generate(Level);
            }

            if (DungeonMap.NextNodes[0].Type == RoomType.RewardRoom) break;

            DungeonMap = DungeonMap.NextNodes[0];
        }
    }

    private void MoveToNextLevel()
    {
        Level++;
        DungeonMap = _dungeonGenerator.Generate(Level);

        GD.Print(DungeonMap.ExportToDot());

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

            var nextNodes = DungeonMap.NextNodes;

            var doorNodes = CurrentRoom.GetNodeOrNull("Doors")?.GetChildren();
            if (doorNodes == null) return;

            var doors = doorNodes
                .OfType<RoomDoor>()
                .ToList();

            if (doors.Count == 0) return;

            // First door - guaranteed to exist and have a valid NextNodes[0]
            var firstDoor = doors[0];
            firstDoor.Icon = GetRoomIcon(nextNodes[0].Type);
            GD.Print($"Left door: {nextNodes[0].Type}");


            // Handle second door if it exists
            if (doors.Count > 1)
            {
                var secondDoor = doors[1];
                var t = nextNodes.Count == 1 ? nextNodes[0].Type : nextNodes[1].Type;
                secondDoor.Icon = GetRoomIcon(t);
                GD.Print($"Right door: {t}");
            }

            // Check room type and handle door opening
            if (DungeonMap.Type is RoomType.BossRoom
                or RoomType.MiniBossRoom
                or RoomType.CombatRoom) return;

            firstDoor.Open();
            if (doors.Count > 1) doors[1].Open();
        }).CallDeferred();


        _player.GlobalPosition = CurrentRoom.GetNodeOrNull<Node2D>("EnterPoint")?.GlobalPosition ?? Vector2.Zero;

        Callable.From(() => GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomEntered)).CallDeferred();

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        _player.Moveable = true;
        LoadingScreen.Visible = false;
    }


    private static Texture2D GetRoomIcon(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.BossRoom => GD.Load<Texture2D>("res://Assets/icons/boss.png"),
            RoomType.MiniBossRoom => GD.Load<Texture2D>("res://Assets/icons/mini_boss.png"),
            RoomType.CombatRoom => GD.Load<Texture2D>("res://Assets/icons/combat.png"),
            RoomType.RewardRoom => GD.Load<Texture2D>("res://Assets/icons/reward.png"),
            RoomType.EventRoom => GD.Load<Texture2D>("res://Assets/icons/event.png"),
            RoomType.ShopRoom => GD.Load<Texture2D>("res://Assets/icons/shop.png"),
            RoomType.StartRoom => null,
            RoomType.EndRoom => null,
            _ => null
        };
    }
}