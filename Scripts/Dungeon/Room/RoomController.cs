using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Entities.Enemies;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

[GlobalClass]
public partial class RoomController : Node
{
    private readonly List<BaseEnemy> _spawnedEnemies = [];
    private int _remainingEnemies;
    private Node _spawnNode;
    private Node2D _spawnPointsContainer;
    [Export] public required PackedScene[] EnemyTypes { get; set; }
    [Export] public NodePath SpawnPointsPath { get; set; } = "SpawnPoints";
    [Export] public bool SpawnOnStart { get; set; } = true;

    public override void _Ready()
    {
        _spawnPointsContainer = GetNodeOrNull<Node2D>(SpawnPointsPath);
        _spawnNode = GetNode("/root/Autoload/Entities");


        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Subscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }

    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
        GlobalEventBus.Instance.Unsubscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }

    public void OnRoomEntered()
    {
        _remainingEnemies = DungeonManager.Instance.Level;

        if (SpawnOnStart)
            // Delay spawning slightly to ensure room is fully set up
            GetTree().CreateTimer(0.5).Timeout += SpawnEnemies;
    }

    private void OnEntityDied(BaseEntity entity)
    {
        if (entity is not BaseEnemy enemy) return;

        if (!_spawnedEnemies.Contains(enemy)) return;
        _spawnedEnemies.Remove(enemy);

        // Check if all enemies are defeated
        if (_spawnedEnemies.Count != 0) return;
        if (_spawnedEnemies.Count == 0 && _remainingEnemies > 0)
        {
            SpawnEnemies();
            return;
        }

        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
    }

    public void SpawnEnemies()
    {
        if (EnemyTypes.Length == 0)
            return;

        var spawnPoints = _spawnPointsContainer.GetChildren().Cast<Node2D>().ToArray();
        if (spawnPoints.Length == 0) return;

        _spawnedEnemies.Clear();

        // Spawn enemies at each spawn point
        foreach (var spawnPoint in spawnPoints)
        {
            if (_remainingEnemies <= 0) break;

            // Pick a random enemy type
            var enemyScene = EnemyTypes[GD.RandRange(0, EnemyTypes.Length - 1)];


            var enemy = enemyScene.Instantiate<BaseEnemy>();
            Callable.From(() => _spawnNode.AddChild(enemy)).CallDeferred();
            enemy.GlobalPosition = spawnPoint.GlobalPosition;

            _spawnedEnemies.Add(enemy);

            _remainingEnemies--;
        }
    }
}