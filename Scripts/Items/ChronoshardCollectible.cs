using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Items;

/// <summary>
///     A collectible item that gives the player chronoshards when picked up
/// </summary>
[GlobalClass]
public partial class ChronoshardCollectible : Area2D
{
    private CollisionShape2D _collisionShape;
    private bool _isBeingCollected;

    private Player _player;
    [Export] public int Value { get; set; } = 10;
    [Export] public bool IsMagnetic { get; set; }
    [Export] public float MagnetRange { get; set; } = 100.0f;
    [Export] public float MagnetSpeed { get; set; } = 200.0f;

    public override void _Ready()
    {
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        // Connect signals
        BodyEntered += OnBodyEntered;

        // Find the player
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }

    public override void _Process(double delta)
    {
        if (IsMagnetic && _player != null && !_isBeingCollected)
        {
            var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance < MagnetRange)
            {
                _isBeingCollected = true;
                MoveToPlayer(delta);
            }
        }

        if (_isBeingCollected) MoveToPlayer(delta);
    }

    private void MoveToPlayer(double delta)
    {
        if (_player == null) return;

        var direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Move towards player
        if (distance > 10.0f) // Stop when close enough to prevent jitter
            GlobalPosition += direction * (float)(MagnetSpeed * delta);
        else
            // Collect the shard if very close
            CollectShard();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player) CollectShard();
    }

    private void CollectShard()
    {
        if (_player == null) return;

        // Add currency to player
        _player.CurrencyManager.AddChronoshards(Value);

        // Publish global event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.CurrencyCollected, Value);

        // Play collection effect or animation
        // TODO: Add particle effects or sound

        // Remove the collectible
        QueueFree();
    }
}