using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon;

public class DungeonNode(RoomType type, int depth)
{
    public RoomType Type { get; set; } = type;
    public int Depth { get; } = depth;
    public List<DungeonNode> NextNodes { get; } = [];
    public List<DungeonNode> PrevNodes { get; } = [];

    /// <summary>
    ///     Exports the dungeon graph to DOT format for visualization.
    /// </summary>
    /// <param name="startNode">The starting node of the dungeon.</param>
    /// <returns>A string in DOT format representing the dungeon graph.</returns>
    public static string ExportToDot(DungeonNode startNode)
    {
        var visited = new HashSet<DungeonNode>();
        var result = new StringBuilder();

        // Start the digraph
        result.AppendLine("digraph Dungeon {");
        result.AppendLine("  node [shape=box];");
        result.AppendLine("  rankdir=TB;");

        // Use a queue for BFS traversal
        var queue = new Queue<DungeonNode>();
        queue.Enqueue(startNode);
        visited.Add(startNode);

        // Track nodes by depth for rank constraints
        var nodesByDepth = new Dictionary<int, List<string>>();

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var nodeId = $"node_{node.GetHashCode()}";
            var nodeColor = GetColorForRoomType(node.Type);

            // Add node to the graph
            result.AppendLine(
                $"  {nodeId} [label=\"{node.Type} (D:{node.Depth})\", style=filled, fillcolor=\"{nodeColor}\"];");

            // Track nodes by depth for rank constraints
            if (!nodesByDepth.ContainsKey(node.Depth)) nodesByDepth[node.Depth] = [];

            nodesByDepth[node.Depth].Add(nodeId);

            // Add edges
            foreach (var nextNode in node.NextNodes)
            {
                var nextNodeId = $"node_{nextNode.GetHashCode()}";
                result.AppendLine($"  {nodeId} -> {nextNodeId};");

                if (visited.Add(nextNode)) queue.Enqueue(nextNode);
            }
        }

        // Add rank constraints to enforce depth layers
        foreach (var depthPair in nodesByDepth.Where(depthPair => depthPair.Value.Count > 1))
        {
            result.Append("  {rank=same; ");
            result.Append(string.Join("; ", depthPair.Value));
            result.AppendLine(";}");
        }

        result.AppendLine("}");
        return result.ToString();
    }

    private static string GetColorForRoomType(RoomType type)
    {
        return type switch
        {
            RoomType.StartRoom => "lightgreen",
            RoomType.EndRoom => "lightyellow",
            RoomType.BossRoom => "orange",
            RoomType.MiniBossRoom => "salmon",
            RoomType.CombatRoom => "lightpink",
            RoomType.RewardRoom => "lightblue",
            RoomType.EventRoom => "lavender",
            RoomType.ShopRoom => "lightcyan",
            _ => "white"
        };
    }
}

public static class DungeonGenerator
{
    // Tracks the last level a mini-boss was generated
    private static int _lastMiniBossLevel = -10;

    /// <summary>
    ///     Generates a DAG of rooms based on the given level.
    /// </summary>
    /// <param name="level">The dungeon level (1-based).</param>
    /// <returns>The start node of the dungeon DAG.</returns>
    public static DungeonNode Generate(int level)
    {
        // Calculate depth based on level
        var depth = Math.Max(3, 2 + level / 2);

        // Number of rooms scales with level, capped at 40
        var numRooms = Math.Min(40, 5 + level * 2);

        // Control branching factor based on level
        var maxBranching = Math.Min(3, 1 + level / 5);

        return GenerateDungeonDAG(numRooms, depth, maxBranching, level);
    }

    private static DungeonNode GenerateDungeonDAG(int numRooms, int depth, int maxBranching, int level)
    {
        var rand = new Random();
        var nodes = new List<DungeonNode>[depth];

        // Initialize lists for each depth level
        for (var i = 0; i < depth; i++) nodes[i] = [];

        // Create start node at depth 0
        var startNode = new DungeonNode(RoomType.StartRoom, 0);
        nodes[0].Add(startNode);

        // Create end node at max depth
        var endNode = new DungeonNode(level % 5 == 0 ? RoomType.BossRoom : RoomType.EndRoom, depth - 1);
        nodes[depth - 1].Add(endNode);

        // Determine if we'll have a mini-boss
        var hasMiniBoss = level > 6 && level - _lastMiniBossLevel > 2;
        DungeonNode miniBossNode = null;

        if (hasMiniBoss)
        {
            // Place mini-boss at approximately 2/3 of the depth
            var miniBossDepth = 2 * depth / 3;
            miniBossNode = new DungeonNode(RoomType.MiniBossRoom, miniBossDepth);
            nodes[miniBossDepth].Add(miniBossNode);
            _lastMiniBossLevel = level;
        }

        // Distribute remaining nodes across depths
        var remainingRooms = numRooms - 2 - (hasMiniBoss ? 1 : 0);

        // Scale number of special rooms based on level - more restrictive
        var specialTypes = new[] { RoomType.RewardRoom, RoomType.EventRoom, RoomType.ShopRoom };
        var targetSpecialRooms = Math.Max(1, level / 6); // Changed from level/5 to level/6
        var specialRoomsAdded = 0;

        // Create a minimum requirement for combat rooms - ensuring they're distributed
        var minCombatRooms = Math.Max(numRooms / 3, 3); // At least 1/3 of rooms should be combat
        var combatRoomsAdded = 0;

        // Fill intermediate depths
        for (var d = 1; d < depth - 1; d++)
        {
            // Skip if this is the mini-boss depth and we already added it
            if (hasMiniBoss && nodes[d].Count > 0 && nodes[d][0].Type == RoomType.MiniBossRoom)
                continue;

            // Determine number of nodes at this depth based on diamond-like shape
            var depthRatio = Math.Min(d, depth - d);
            var nodesAtDepth = Math.Min(remainingRooms, 1 + depthRatio);

            // Cap based on branching factor
            nodesAtDepth = Math.Min(nodesAtDepth, maxBranching * Math.Max(1, nodes[d - 1].Count));

            for (var i = 0; i < nodesAtDepth; i++)
            {
                RoomType roomType;

                // Calculate probability of special room based on depth percentage
                var depthPercentage = (double)d / (depth - 1);

                // Distribution formula: probability increases with depth, but not linearly
                // This creates balanced distribution throughout the dungeon
                var specialRoomProb = 0.1 + depthPercentage * 0.4; // 10% at start, up to 50% at end

                // Adjust probability if we're behind or ahead on target counts
                if (specialRoomsAdded < targetSpecialRooms * (depthPercentage + 0.2))
                    specialRoomProb += 0.2; // Boost probability if behind target
                else if
                    (specialRoomsAdded >
                     targetSpecialRooms * depthPercentage)
                    specialRoomProb -= 0.2; // Reduce probability if ahead of target

                // Adjust combat room probability similarly
                var expectedCombatRoomsAtThisPoint = minCombatRooms * depthPercentage;
                var needMoreCombat = combatRoomsAdded < expectedCombatRoomsAtThisPoint;

                // Determine room type based on calculated probabilities
                if (specialRoomsAdded < targetSpecialRooms && rand.NextDouble() < specialRoomProb)
                {
                    roomType = specialTypes[rand.Next(specialTypes.Length)];
                    specialRoomsAdded++;
                }
                else if (needMoreCombat || rand.NextDouble() < 0.7) // Favor combat rooms generally
                {
                    roomType = RoomType.CombatRoom;
                    combatRoomsAdded++;
                }
                else
                {
                    // Random choice between combat and special
                    if (rand.NextDouble() < 0.5 && specialRoomsAdded < targetSpecialRooms)
                    {
                        roomType = specialTypes[rand.Next(specialTypes.Length)];
                        specialRoomsAdded++;
                    }
                    else
                    {
                        roomType = RoomType.CombatRoom;
                        combatRoomsAdded++;
                    }
                }

                nodes[d].Add(new DungeonNode(roomType, d));
                remainingRooms--;

                if (remainingRooms <= 0)
                    break;
            }

            if (remainingRooms <= 0)
                break;
        }

        // Balance final distributions if needed
        BalanceRoomDistribution(nodes, depth, targetSpecialRooms, minCombatRooms);

        // Connect nodes - create edges in the graph
        // Start with level 0 -> level 1
        if (nodes[1].Count > 0)
            foreach (var node in nodes[1])
                ConnectNodes(startNode, node);
        else if (depth > 1)
            // If level 1 is empty, connect directly to the end
            ConnectNodes(startNode, endNode);

        // Middle layers
        for (var d = 1; d < depth - 1; d++)
        {
            if (nodes[d].Count == 0) continue;

            // If miniboss is at this level, all paths must go through it
            if (hasMiniBoss && miniBossNode.Depth == d)
            {
                // Connect previous layer to miniboss
                if (d > 0)
                    foreach (var prevNode in nodes[d - 1])
                        ConnectNodes(prevNode, miniBossNode);

                // Connect miniboss to next layer or end
                if (d < depth - 2)
                    foreach (var nextNode in nodes[d + 1])
                        ConnectNodes(miniBossNode, nextNode);
                else
                    ConnectNodes(miniBossNode, endNode);
            }
            else
            {
                // Create connections with controlled randomness
                foreach (var currentNode in nodes[d])
                    // Connect to next layer
                    if (d < depth - 2 && nodes[d + 1].Count > 0)
                    {
                        // Determine how many nodes to connect to (1 to maxBranching)
                        var connectionsCount = Math.Min(nodes[d + 1].Count,
                            rand.Next(1, Math.Min(maxBranching, nodes[d + 1].Count) + 1));

                        // Create a weighted sampling based on node types
                        // This ensures varied paths through the dungeon
                        var nextNodes = nodes[d + 1].OrderBy(_ => rand.Next()).ToList();

                        // Modified: Prioritize connecting to different room types from each other
                        // This enforces each connection leads to a different room type
                        var selectedNextNodes = new List<DungeonNode>();
                        foreach (var candidate in nextNodes.Where(candidate =>
                                     selectedNextNodes.All(n => n.Type != candidate.Type)))
                        {
                            selectedNextNodes.Add(candidate);
                            if (selectedNextNodes.Count >= connectionsCount)
                                break;
                        }

                        // If we couldn't find enough unique types, add more nodes up to the connection count
                        if (selectedNextNodes.Count < connectionsCount && nextNodes.Count > selectedNextNodes.Count)
                        {
                            var remainingOptions = nextNodes
                                .Where(n => !selectedNextNodes.Contains(n))
                                .Take(connectionsCount - selectedNextNodes.Count);

                            selectedNextNodes.AddRange(remainingOptions);
                        }

                        foreach (var nextNode in selectedNextNodes) ConnectNodes(currentNode, nextNode);
                    }
                    // Connect to end if next layer is empty or this is the second-to-last layer
                    else if (d == depth - 2 || nodes[d + 1].Count == 0)
                    {
                        ConnectNodes(currentNode, endNode);
                    }
            }
        }

        // Ensure all nodes have connections
        EnsureConnectionsToEnd(nodes, endNode, depth);

        return startNode;
    }

    private static void BalanceRoomDistribution(List<DungeonNode>[] nodes, int depth, int targetSpecialRooms,
        int minCombatRooms)
    {
        var rand = new Random();
        var specialTypes = new[] { RoomType.RewardRoom, RoomType.EventRoom, RoomType.ShopRoom };

        // Count current room types
        var allNodes = nodes.SelectMany(n => n).ToList();
        var specialRoomCount = allNodes.Count(n => specialTypes.Contains(n.Type));
        var combatRoomCount = allNodes.Count(n => n.Type == RoomType.CombatRoom);

        // Fix combat room distribution if needed
        if (combatRoomCount < minCombatRooms)
        {
            // Find special rooms to convert to combat rooms
            var specialRoomsToConvert = allNodes
                .Where(n => specialTypes.Contains(n.Type))
                .OrderBy(_ => rand.Next())
                .Take(Math.Min(specialRoomCount - targetSpecialRooms, minCombatRooms - combatRoomCount))
                .ToList();

            foreach (var node in specialRoomsToConvert) node.Type = RoomType.CombatRoom;
        }

        // Fix special room distribution if needed
        var adjustedSpecialCount = allNodes.Count(n => specialTypes.Contains(n.Type));

        if (adjustedSpecialCount > targetSpecialRooms)
        {
            // Find excess special rooms to convert to combat
            var excessSpecialRooms = allNodes
                .Where(n => specialTypes.Contains(n.Type))
                .OrderBy(_ => rand.Next())
                .Take(adjustedSpecialCount - targetSpecialRooms)
                .ToList();

            foreach (var node in excessSpecialRooms) node.Type = RoomType.CombatRoom;
        }
        else if (adjustedSpecialCount < targetSpecialRooms)
        {
            // Find combat rooms to convert to special rooms
            // Prioritize mid to late dungeon depths
            var combatRoomsToConvert = allNodes
                .Where(n => n.Type == RoomType.CombatRoom)
                .OrderByDescending(n => (double)n.Depth / depth) // Prefer rooms deeper in the dungeon
                .ThenBy(_ => rand.Next())
                .Take(Math.Min(combatRoomCount - minCombatRooms, targetSpecialRooms - adjustedSpecialCount))
                .ToList();

            foreach (var node in combatRoomsToConvert) node.Type = specialTypes[rand.Next(specialTypes.Length)];
        }

        // Ensure we have at least one of each special room type if possible
        foreach (var specialType in specialTypes)
        {
            if (allNodes.Any(n => n.Type == specialType) ||
                allNodes.Count(n => n.Type == RoomType.CombatRoom) <= minCombatRooms) continue;
            {
                // Find a combat room to convert, preferably in the middle or late dungeon
                var roomToConvert = allNodes
                    .Where(n => n.Type == RoomType.CombatRoom)
                    .OrderByDescending(n => (double)n.Depth / depth >= 0.4 && (double)n.Depth / depth <= 0.8)
                    .ThenBy(_ => rand.Next())
                    .FirstOrDefault();

                if (roomToConvert != null) roomToConvert.Type = specialType;
            }
        }
    }

    private static void ConnectNodes(DungeonNode from, DungeonNode to)
    {
        // Check if a node with the same room type already exists in NextNodes
        if (from.NextNodes.Any(node => node.Type == to.Type))
            return; // Don't connect if a node with same room type exists

        // Add connections
        if (!from.NextNodes.Contains(to)) from.NextNodes.Add(to);

        if (!to.PrevNodes.Contains(from)) to.PrevNodes.Add(from);
    }

    // Ensure all nodes have a path to the end
    private static void EnsureConnectionsToEnd(List<DungeonNode>[] nodes, DungeonNode endNode, int depth)
    {
        var rand = new Random();

        // Check each layer from second-to-last to first
        for (var d = depth - 2; d >= 0; d--)
            foreach (var node in nodes[d].Where(node => node.NextNodes.Count == 0))
                // Node has no outgoing connections, connect to end or next layer
                if (d == depth - 2)
                {
                    ConnectNodes(node, endNode);
                }
                else if (nodes[d + 1].Count > 0)
                {
                    // Try to connect to nodes with different types for variety
                    var possibleNextNodes = nodes[d + 1]
                        .Where(n => node.NextNodes.All(existing => existing.Type != n.Type))
                        .OrderBy(_ => rand.Next())
                        .ToList();

                    if (possibleNextNodes.Count > 0)
                    {
                        var nextNode = possibleNextNodes[0];
                        ConnectNodes(node, nextNode);
                    }
                    else
                    {
                        // If no room with a different type is available, connect to any node
                        var nextNode = nodes[d + 1][rand.Next(nodes[d + 1].Count)];
                        // Force connection despite room type
                        if (!node.NextNodes.Contains(nextNode)) node.NextNodes.Add(nextNode);

                        if (!nextNode.PrevNodes.Contains(node)) nextNode.PrevNodes.Add(node);
                    }
                }
                else
                {
                    // Find the next non-empty layer
                    for (var nextD = d + 1; nextD < depth; nextD++)
                    {
                        if (nodes[nextD].Count > 0)
                        {
                            var possibleNextNodes = nodes[nextD]
                                .Where(n => node.NextNodes.All(existing => existing.Type != n.Type))
                                .OrderBy(_ => rand.Next())
                                .ToList();

                            if (possibleNextNodes.Count > 0)
                            {
                                var nextNode = possibleNextNodes[0];
                                ConnectNodes(node, nextNode);
                            }
                            else
                            {
                                // If no room with a different type is available, connect to any node
                                var nextNode = nodes[nextD][rand.Next(nodes[nextD].Count)];
                                // Force connection despite room type
                                if (!node.NextNodes.Contains(nextNode)) node.NextNodes.Add(nextNode);

                                if (!nextNode.PrevNodes.Contains(node)) nextNode.PrevNodes.Add(node);
                            }

                            break;
                        }

                        if (nextD == depth - 1)
                            // If reached the end with no nodes, connect to end
                            ConnectNodes(node, endNode);
                    }
                }
    }
}