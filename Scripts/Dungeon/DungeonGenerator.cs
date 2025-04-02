using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon;

public class DungeonGenerator
{
    private readonly Random _random;
    private readonly PathGenerator _pathGenerator;
    private readonly BranchManager _branchManager;
    private readonly RoomBalancer _roomBalancer;

    /// <summary>
    /// Initializes a new instance of the DungeonGenerator with customizable parameters.
    /// </summary>
    /// <param name="seed">Optional seed for reproducible random generation.</param>
    /// <param name="branchProbability">Probability of adding a branch from a node (0.0 to 1.0).</param>
    /// <param name="mergeProbability">Probability of merging two paths (0.0 to 1.0).</param>
    /// <param name="roomDistribution">Custom room type distribution ranges.</param>
    public DungeonGenerator(
        int? seed = null,
        double branchProbability = 0.3,
        double mergeProbability = 0.4,
        Dictionary<RoomType, (double min, double max)> roomDistribution = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _pathGenerator = new PathGenerator();
        _branchManager = new BranchManager(branchProbability, mergeProbability);
        _roomBalancer = new RoomBalancer(roomDistribution ?? new Dictionary<RoomType, (double min, double max)>
        {
            { RoomType.CombatRoom, (0.40, 0.50) },
            { RoomType.RewardRoom, (0.15, 0.20) },
            { RoomType.EventRoom, (0.15, 0.20) },
            { RoomType.ShopRoom, (0.05, 0.10) },
            { RoomType.MiniBossRoom, (0.05, 0.10) }
        });
    }

    /// <summary>
    /// Generates a dungeon structure as a directed acyclic graph.
    /// </summary>
    /// <param name="level">The dungeon level, affecting size and difficulty.</param>
    /// <returns>The starting node of the dungeon.</returns>
    public DungeonNode Generate(int level)
    {
        var dungeonSize = CalculateDungeonSize(level);
        var startRoom = new DungeonNode(RoomType.StartRoom, 0);

        // Generate the dungeon structure
        var endRoom = PathGenerator.GenerateMainPath(startRoom, dungeonSize, level);
        _branchManager.AddBranchesAndMerges(startRoom, _random);
        DungeonUtils.CalculateDepths(startRoom); // Ensure accurate depths
        _roomBalancer.BalanceRoomTypes(startRoom, _random);
        EnsureProperConnectivity(startRoom);

        return startRoom;
    }

    private int CalculateDungeonSize(int level)
    {
        return 15 + level * 2; // Simple scaling based on level
    }

    private void EnsureProperConnectivity(DungeonNode startNode)
    {
        var allNodes = DungeonUtils.CollectAllNodes(startNode);
        var endNodes = allNodes.Where(n => n.Type == RoomType.EndRoom).ToList();
        var deadEnds = allNodes.Where(n => n.NextNodes.Count == 0 && n.Type != RoomType.EndRoom).ToList();

        foreach (var deadEnd in deadEnds)
        {
            var candidates = allNodes
                .Where(n => n.Depth > deadEnd.Depth && !DungeonUtils.IsReachable(deadEnd, n) &&
                            !DungeonUtils.WouldCreateCycle(deadEnd, n))
                .OrderBy(n => n.Depth - deadEnd.Depth)
                .Take(3)
                .ToList();

            if (candidates.Count != 0)
            {
                var target = candidates[_random.Next(candidates.Count)];
                DungeonUtils.ConnectNodes(deadEnd, target);
            }
            else if (endNodes.Count != 0)
            {
                var endRoom = endNodes[_random.Next(endNodes.Count)];
                if (!DungeonUtils.IsReachable(deadEnd, endRoom) && !DungeonUtils.WouldCreateCycle(deadEnd, endRoom))
                {
                    DungeonUtils.ConnectNodes(deadEnd, endRoom);
                }
            }
        }
    }
}