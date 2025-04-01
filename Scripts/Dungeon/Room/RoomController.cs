using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Entities.Enemies;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

[GlobalClass]
public partial class RoomController : Node
{
    [Export] public required PackedScene[] EnemyTypes { get; set; }
    [Export] public NodePath SpawnPointsPath { get; set; } = "SpawnPoints";
    [Export] public bool SpawnOnStart { get; set; } = true;
    
    private readonly List<BaseEnemy> _spawnedEnemies = [];
    private bool _roomCleared;
    private Node2D _spawnPointsContainer;
    
    public override void _Ready()
    {
        GlobalEventBus.Instance.Subscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
        
        _spawnPointsContainer = GetNodeOrNull<Node2D>(SpawnPointsPath);
    }
    
    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe<BaseEntity>(GlobalEventVariant.EntityDied, OnEntityDied);
    }
    
    public void OnRoomEntered()
    {
        if (SpawnOnStart)
        {
            // Delay spawning slightly to ensure room is fully set up
            GetTree().CreateTimer(0.2).Timeout += SpawnEnemies;
        }
    }
    
    private void OnEntityDied(BaseEntity entity)
    {
        if (entity is not BaseEnemy enemy) return;

        if (!_spawnedEnemies.Contains(enemy)) return;
        _spawnedEnemies.Remove(enemy);
            
        // Check if all enemies are defeated
        if (_spawnedEnemies.Count != 0 || _roomCleared) return;
        _roomCleared = true;
        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
    }
    
    public void SpawnEnemies()
    {
        if (EnemyTypes.Length == 0)
            return;
            
        var spawnPoints = _spawnPointsContainer.GetChildren().Cast<Node2D>().ToArray();
        if (spawnPoints.Length == 0) return;
        
        _roomCleared = false;
        _spawnedEnemies.Clear();
        
        // Spawn enemies at each spawn point
        foreach (var spawnPoint in spawnPoints)
        {
            // Pick a random enemy type
            var enemyScene = EnemyTypes[GD.RandRange(0, EnemyTypes.Length - 1)];

            {
                var enemy = enemyScene.Instantiate<BaseEnemy>();
                GetTree().Root.GetNode("/root/Autoload/Entities").AddChild(enemy);
                enemy.GlobalPosition = spawnPoint.GlobalPosition;
                
                _spawnedEnemies.Add(enemy);
            }
        }
        
        // Emit the RoomStart event when enemies are spawned
        GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomStarted);
    }
    
    // Public method to manually trigger enemy spawning
    public void TriggerEnemySpawn()
    {
        SpawnEnemies();
    }
}