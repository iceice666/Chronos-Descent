using ChronosDescent.Scripts.Core.Blessing;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

/// <summary>
///     Main script for the blessing reward room scene
/// </summary>
[GlobalClass]
public partial class BlessingRewardRoom : Node2D
{
    private BlessingRewardRoomController _roomController;
    private Node _doors;
    private Node2D _enterPoint;
    
    [Export] public NodePath EnterPointPath { get; set; } = "EnterPoint";
    [Export] public NodePath DoorsPath { get; set; } = "Doors";
    [Export] public NodePath RoomControllerPath { get; set; } = "BlessingRoomController";

    public override void _Ready()
    {
        // Get references to key nodes
        _enterPoint = GetNode<Node2D>(EnterPointPath);
        _doors = GetNode(DoorsPath);
        _roomController = GetNode<BlessingRewardRoomController>(RoomControllerPath);

        // Subscribe to room events
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Subscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Unsubscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);
    }

    /// <summary>
    ///     Handle room entry
    /// </summary>
    private void OnRoomEntered()
    {
        // Close doors when entering the room
        CloseDoors();
        
        // Room controller will handle initializing the blessing items automatically
    }

    /// <summary>
    ///     Handle blessing selection
    /// </summary>
    private void OnBlessingSelected(Blessing blessing)
    {
        // Play blessing effect and sound
        PlayBlessingSound(blessing);
    }
    
    /// <summary>
    ///     Handle room cleared event
    /// </summary>
    private void OnRoomCleared()
    {
        // Open doors to allow the player to leave
        OpenDoors();
    }

    /// <summary>
    ///     Play a sound effect for the selected blessing
    /// </summary>
    private void PlayBlessingSound(Blessing blessing)
    {
        // Play sound based on blessing rarity
        var audioPlayer = new AudioStreamPlayer();

        // In a real implementation, we would load different sounds based on rarity
        // For now, we'll just use a placeholder sound

        AddChild(audioPlayer);
        audioPlayer.Play();

        // Remove audio player when finished
        audioPlayer.Finished += () => audioPlayer.QueueFree();
    }

    /// <summary>
    ///     Open doors to allow the player to leave
    /// </summary>
    private void OpenDoors()
    {
        // Open each door in the doors node
        foreach (var door in _doors.GetChildren())
        {
            if (door is not RoomDoor roomDoor) continue;
            roomDoor.Open();
        }
    }
    
    /// <summary>
    ///     Close doors when entering the room
    /// </summary>
    private void CloseDoors()
    {
        // Close each door in the doors node
        foreach (var door in _doors.GetChildren())
        {
            if (door is not RoomDoor roomDoor) continue;
            roomDoor.Close();
        }
    }
}