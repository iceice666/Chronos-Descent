using ChronosDescent.Scripts.Core.Blessing;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Main script for the blessing reward room scene
/// </summary>
[GlobalClass]
public partial class BlessingRewardRoom : Node2D
{
    private Player _player;
    private BlessingRewardRoomController _blessingUI;
    private Node _doors;
    private Node2D _enterPoint;
    [Export] public NodePath EnterPointPath { get; set; } = "EnterPoint";
    [Export] public NodePath DoorsPath { get; set; } = "Doors";
    [Export] public NodePath BlessingUIPath { get; set; } = "BlessingUI";

    public override void _Ready()
    {
        _player = GetNode<Player>("/root/Dungeon/Player");
        
        // Get references to key nodes
        _enterPoint = GetNode<Node2D>(EnterPointPath);
        _doors = GetNode(DoorsPath);
        _blessingUI = GetNode<BlessingRewardRoomController>(BlessingUIPath);

        // Subscribe to room events
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
        GlobalEventBus.Instance.Subscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomStarted, OnRoomEntered);
        GlobalEventBus.Instance.Unsubscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
    }

    /// <summary>
    ///     Handle room entry
    /// </summary>
    private void OnRoomEntered()
    {
        // Display blessing UI after a short delay
        GetTree().CreateTimer(0.5).Timeout += () =>
        {
            if (_blessingUI == null) return;
            _blessingUI.Visible = true;

            // Lock player movement while choosing blessing
            _player.Moveable = false;
        };
    }

    /// <summary>
    ///     Handle blessing selection
    /// </summary>
    private void OnBlessingSelected(Blessing blessing)
    {
        // Show visual effect based on blessing
        CreateBlessingEffect(blessing);

        // Play sound effect
        PlayBlessingSound(blessing);

        // Open doors and allow player to leave
        OpenDoors();
    }

    /// <summary>
    ///     Create a visual effect for the selected blessing
    /// </summary>
    private void CreateBlessingEffect(Blessing blessing)
    {
        // Create a particles effect at the center of the room
        var particles = new GpuParticles2D();
        particles.Position = _enterPoint.Position;

        // Set up particles based on blessing category
        var material = new ParticleProcessMaterial();

        switch (blessing.Category)
        {
            case BlessingCategory.Offensive:
                material.Color = new Color(1.0f, 0.3f, 0.3f); // Red
                break;
            case BlessingCategory.Defensive:
                material.Color = new Color(0.3f, 0.7f, 1.0f); // Blue
                break;
            case BlessingCategory.Utility:
                material.Color = new Color(1.0f, 0.8f, 0.2f); // Yellow
                break;
            case BlessingCategory.Movement:
                material.Color = new Color(0.2f, 0.9f, 0.5f); // Green
                break;
        }

        particles.ProcessMaterial = material;
        particles.Emitting = true;
        particles.OneShot = true;

        AddChild(particles);

        // Remove particles after effects finish
        GetTree().CreateTimer(3.0).Timeout += () => particles.QueueFree();
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
        // In a real implementation, this would be handled by the RoomCleared event
        // which the BlessingRewardRoomController already triggers
        
        _player.Moveable = true;

        // Display a message to guide the player
        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            "Blessing received. Proceed to the next room.");
    }
}
