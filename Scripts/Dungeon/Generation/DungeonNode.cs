using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon.Generation;

public class DungeonNode(RoomType type, int depth)
{
    public RoomType Type { get; set; } = type;
    public int Depth { get; set; } = depth;
    public List<DungeonNode> NextNodes { get; } = [];
    public List<DungeonNode> PrevNodes { get; } = [];

    /// <summary>
    ///     Exports the dungeon graph to DOT format for visualization.
    /// </summary>
    /// <param name="startNode">The starting node of the dungeon.</param>
    /// <returns>A string in DOT format representing the dungeon graph.</returns>
    public string ExportToDot()
    {
        var visited = new HashSet<DungeonNode>();
        var result = new StringBuilder();

        // Start the digraph
        result.AppendLine("digraph Dungeon {");
        result.AppendLine("  node [shape=box];");
        result.AppendLine("  rankdir=TB;");

        // Use a queue for BFS traversal
        var queue = new Queue<DungeonNode>();
        queue.Enqueue(this);
        visited.Add(this);

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