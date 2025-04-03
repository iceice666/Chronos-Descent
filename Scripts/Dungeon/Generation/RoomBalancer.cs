using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon.Generation;

public class RoomBalancer(Dictionary<RoomType, (double min, double max)> roomDistribution)
{
    /// <summary>
    ///     Balances the distribution of room types across the dungeon.
    /// </summary>
    /// <param name="startNode">The starting node of the dungeon.</param>
    /// <param name="random">The random number generator.</param>
    public void BalanceRoomTypes(DungeonNode startNode, Random random)
    {
        var allNodes = DungeonUtils.CollectAllNodes(startNode);
        var fixedRoomTypes = new HashSet<RoomType> { RoomType.StartRoom, RoomType.EndRoom, RoomType.BossRoom };
        var modifiableNodes = allNodes.Where(n => !fixedRoomTypes.Contains(n.Type)).ToList();

        // Reset modifiable nodes to CombatRoom
        foreach (var node in modifiableNodes) node.Type = RoomType.CombatRoom;

        var targetCounts = CalculateTargetRoomCounts(modifiableNodes.Count, random);
        PlaceMiniBossRooms(modifiableNodes, targetCounts[RoomType.MiniBossRoom], random);
        DistributeRoomTypes(modifiableNodes, targetCounts, random);
        OptimizeShopPlacements(startNode, random);
    }

    private Dictionary<RoomType, int> CalculateTargetRoomCounts(int totalRooms, Random random)
    {
        var targetCounts = new Dictionary<RoomType, int>();
        var assignedRooms = 0;

        foreach (var kvp in roomDistribution)
        {
            var minPct = kvp.Value.min;
            var maxPct = kvp.Value.max;
            var minCount = (int)Math.Floor(totalRooms * minPct);
            var maxCount = (int)Math.Ceiling(totalRooms * maxPct);
            var count = random.Next(minCount, maxCount + 1);
            targetCounts[kvp.Key] = count;
            assignedRooms += count;
        }

        // Adjust for total room count
        if (assignedRooms == totalRooms) return targetCounts;
        var difference = totalRooms - assignedRooms;
        targetCounts[RoomType.CombatRoom] += difference; // CombatRoom absorbs the difference
        if (targetCounts[RoomType.CombatRoom] < 0) targetCounts[RoomType.CombatRoom] = 0; // Ensure non-negative

        return targetCounts;
    }

    private static void PlaceMiniBossRooms(List<DungeonNode> modifiableNodes, int targetCount, Random random)
    {
        var nodesByDepth = modifiableNodes.GroupBy(n => n.Depth).OrderBy(g => g.Key).ToList();
        if (nodesByDepth.Count < 3 || targetCount <= 0) return;

        var maxDepth = nodesByDepth.Last().Key;
        var spacing = (double)maxDepth / (targetCount + 1);

        for (var i = 1; i <= targetCount && i * spacing <= maxDepth; i++)
        {
            var targetDepth = (int)Math.Round(i * spacing);
            var closestGroup = nodesByDepth.OrderBy(g => Math.Abs(g.Key - targetDepth)).First();
            var candidates = closestGroup.Where(n => n.Type == RoomType.CombatRoom).ToList();
            if (candidates.Any()) candidates[random.Next(candidates.Count)].Type = RoomType.MiniBossRoom;
        }
    }

    private static void DistributeRoomTypes(List<DungeonNode> modifiableNodes, Dictionary<RoomType, int> targetCounts,
        Random random)
    {
        var currentCounts = DungeonUtils.CountRoomTypes(modifiableNodes);
        var roomsToAdd = new Dictionary<RoomType, int>();
        foreach (var kvp in targetCounts)
        {
            currentCounts.TryGetValue(kvp.Key, out var currentCount);
            roomsToAdd[kvp.Key] = Math.Max(0, kvp.Value - currentCount);
        }

        var availableNodes = modifiableNodes.Where(n => n.Type == RoomType.CombatRoom).ToList();
        DungeonUtils.ShuffleList(availableNodes, random);

        var index = 0;
        foreach (var roomType in new[] { RoomType.RewardRoom, RoomType.EventRoom, RoomType.ShopRoom })
        {
            var count = roomsToAdd[roomType];
            for (var i = 0; i < count && index < availableNodes.Count; i++) availableNodes[index++].Type = roomType;
        }
    }

    private static void OptimizeShopPlacements(DungeonNode startNode, Random random)
    {
        var allNodes = DungeonUtils.CollectAllNodes(startNode);
        var shopNodes = allNodes.Where(n => n.Type == RoomType.ShopRoom).ToList();

        foreach (var shopNode in shopNodes)
        {
            var nextNodes = DungeonUtils.GetNodesWithinDistance(shopNode, 2, true);
            var hasDifficultEncounter =
                nextNodes.Any(n => n.Type is RoomType.MiniBossRoom or RoomType.BossRoom);
            if (hasDifficultEncounter) continue;

            var potentialSwapNodes =
                allNodes.Where(n => n.Type is RoomType.CombatRoom or RoomType.EventRoom).ToList();
            foreach (var node in from node in potentialSwapNodes
                     let ahead = DungeonUtils.GetNodesWithinDistance(node, 2, true)
                     where ahead.Any(n => n.Type is RoomType.MiniBossRoom or RoomType.BossRoom)
                     select node)
            {
                (shopNode.Type, node.Type) = (node.Type, shopNode.Type);
                break;
            }
        }
    }
}