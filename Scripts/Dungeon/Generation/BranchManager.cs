using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon.Generation;

public class BranchManager(double branchProbability, double mergeProbability)
{
    /// <summary>
    /// Adds branches and merges to the dungeon starting from the given node.
    /// </summary>
    /// <param name="startRoom">The starting node of the dungeon.</param>
    /// <param name="random">The random number generator.</param>
    public void AddBranchesAndMerges(DungeonNode startRoom, Random random)
    {
        var allNodes = DungeonUtils.CollectAllNodes(startRoom);
        var depthGroups = allNodes.GroupBy(n => n.Depth).ToDictionary(g => g.Key, g => g.ToList());
        var maxDepth = depthGroups.Keys.Max();

        // Add branches
        for (var depth = 1; depth < maxDepth - 1; depth++)
        {
            if (!depthGroups.ContainsKey(depth)) continue;

            foreach (var node in depthGroups[depth])
            {
                if (node.Type is RoomType.BossRoom or RoomType.EndRoom) continue;

                if (random.NextDouble() < branchProbability)
                {
                    var branchLength = random.Next(1, 4);
                    branchLength = Math.Min(branchLength, maxDepth - depth - 1);
                    if (branchLength > 0)
                    {
                        CreateBranch(node, branchLength, depthGroups, random);
                    }
                }
            }
        }

        // Add merges
        for (var depth = 2; depth < maxDepth; depth++)
        {
            if (!depthGroups.ContainsKey(depth) || depthGroups[depth].Count < 2) continue;

            var nodes = depthGroups[depth].Where(n => n.Type != RoomType.BossRoom && n.Type != RoomType.EndRoom)
                .ToList();
            for (var i = 0; i < nodes.Count - 1; i++)
            {
                if (random.NextDouble() > mergeProbability) continue;
                
                var mergeDepth = depth + 1 + random.Next(2);
                if (!depthGroups.TryGetValue(mergeDepth, out var group)) continue;
                
                var targetNode = group.FirstOrDefault(n =>
                    n.Type != RoomType.BossRoom && n.Type != RoomType.EndRoom);
                if (targetNode == null 
                    || DungeonUtils.IsReachable(nodes[i], targetNode) 
                    || DungeonUtils.IsReachable(nodes[i + 1], targetNode)) continue;
                
                DungeonUtils.ConnectNodes(nodes[i], targetNode);
                DungeonUtils.ConnectNodes(nodes[i + 1], targetNode);
            }
        }
    }

    private void CreateBranch(DungeonNode fromNode, int length, Dictionary<int, List<DungeonNode>> depthGroups,
        Random random)
    {
        var current = fromNode;
        var startDepth = fromNode.Depth;

        for (var i = 1; i <= length; i++)
        {
            var newDepth = startDepth + i;
            var newNode = new DungeonNode(RoomType.CombatRoom, newDepth);
            DungeonUtils.ConnectNodes(current, newNode);

            if (!depthGroups.ContainsKey(newDepth))
            {
                depthGroups[newDepth] = new List<DungeonNode>();
            }

            depthGroups[newDepth].Add(newNode);

            current = newNode;

            // Possible early merge
            if (i >= length || !(random.NextDouble() < mergeProbability / 2)) continue;
            if (!depthGroups.ContainsKey(newDepth + 1)) continue;
            
            var targetNode = depthGroups[newDepth + 1].FirstOrDefault(n =>
                n.Type != RoomType.MiniBossRoom && n.Type != RoomType.BossRoom);
            if (targetNode == null
                || DungeonUtils.IsReachable(newNode, targetNode)
                || DungeonUtils.WouldCreateCycle(newNode, targetNode)) continue;
            
            DungeonUtils.ConnectNodes(newNode, targetNode);
            break;
        }
    }
}