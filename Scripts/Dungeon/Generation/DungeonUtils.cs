using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon.Generation;

public static class DungeonUtils
{
    /// <summary>
    /// Connects two nodes by adding 'to' to 'from's NextNodes and 'from' to 'to's PrevNodes.
    /// </summary>
    public static void ConnectNodes(DungeonNode from, DungeonNode to)
    {
        if (from.NextNodes.Contains(to)) return;
        from.NextNodes.Add(to);
        to.PrevNodes.Add(from);
    }

    /// <summary>
    /// Collects all nodes reachable from the start node using BFS.
    /// </summary>
    public static List<DungeonNode> CollectAllNodes(DungeonNode startNode)
    {
        var result = new List<DungeonNode>();
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);
            foreach (var next in node.NextNodes.Where(next => visited.Add(next)))
            {
                queue.Enqueue(next);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if there is a path from 'from' to 'to' using BFS.
    /// </summary>
    public static bool IsReachable(DungeonNode from, DungeonNode to)
    {
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node == to) return true;
            foreach (var next in node.NextNodes.Where(next => visited.Add(next)))
            {
                queue.Enqueue(next);
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if connecting 'from' to 'to' would create a cycle.
    /// </summary>
    public static bool WouldCreateCycle(DungeonNode from, DungeonNode to)
    {
        return IsReachable(to, from);
    }

    /// <summary>
    /// Counts the number of each room type in the given list of nodes.
    /// </summary>
    public static Dictionary<RoomType, int> CountRoomTypes(List<DungeonNode> nodes)
    {
        return nodes
            .GroupBy(n => n.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Shuffles a list in place using the Fisher-Yates algorithm.
    /// </summary>
    public static void ShuffleList<T>(List<T> list, Random random)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    /// <summary>
    /// Gets nodes within a specified distance from the given node, in the specified direction.
    /// </summary>
    public static List<DungeonNode> GetNodesWithinDistance(DungeonNode node, int distance, bool forward)
    {
        var result = new List<DungeonNode>();
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<(DungeonNode Node, int Distance)>();

        queue.Enqueue((node, 0));
        visited.Add(node);

        while (queue.Count > 0)
        {
            var (current, currentDistance) = queue.Dequeue();
            if (currentDistance > 0)
            {
                result.Add(current);
            }

            if (currentDistance >= distance) continue;
            var neighbors = forward ? current.NextNodes : current.PrevNodes;
            foreach (var neighbor in neighbors.Where(neighbor => visited.Add(neighbor)))
            {
                queue.Enqueue((neighbor, currentDistance + 1));
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates the depth of each node from the start node using BFS, accounting for the longest path in a DAG.
    /// </summary>
    public static void CalculateDepths(DungeonNode startNode)
    {
        var queue = new System.Collections.Generic.Queue<DungeonNode>();
        var visited = new System.Collections.Generic.HashSet<DungeonNode>();
        startNode.Depth = 0;
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var next in node.NextNodes)
            {
                if (!visited.Contains(next))
                {
                    next.Depth = node.Depth + 1;
                    visited.Add(next);
                    queue.Enqueue(next);
                }
                else
                {
                    // Update depth if a longer path is found
                    next.Depth = Math.Max(next.Depth, node.Depth + 1);
                }
            }
        }
    }
}