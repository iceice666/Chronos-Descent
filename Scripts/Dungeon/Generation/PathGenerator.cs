using System;
using ChronosDescent.Scripts.Dungeon.Room;

namespace ChronosDescent.Scripts.Dungeon.Generation;

public class PathGenerator
{
    /// <summary>
    /// Generates the main path of the dungeon from the start room.
    /// </summary>
    /// <param name="startRoom">The starting node of the dungeon.</param>
    /// <param name="dungeonSize">The target size of the dungeon.</param>
    /// <param name="level">The dungeon level, affecting boss room count.</param>
    /// <param name="random">The random number generator.</param>
    /// <returns>The last node in the main path (typically the end room).</returns>
    public static DungeonNode GenerateMainPath(DungeonNode startRoom, int dungeonSize, int level)
    {
        var mainPathLength = dungeonSize / 2; // Main path is roughly half the total size
        var current = startRoom;

        // Determine boss room positions based on level
        var numBossRooms = level > 5 ? 2 : 1;
        var bossPositions = numBossRooms == 1
            ? new[] { mainPathLength - 1 }
            : new[] { mainPathLength / 2, mainPathLength - 1 };

        // Generate the main path nodes
        for (var i = 1; i < mainPathLength; i++)
        {
            var roomType = Array.IndexOf(bossPositions, i) >= 0 ? RoomType.BossRoom : RoomType.CombatRoom; // Default, balanced later

            var newNode = new DungeonNode(roomType, i);
            DungeonUtils.ConnectNodes(current, newNode);
            current = newNode;
        }

        // Add the end room
        var endRoom = new DungeonNode(RoomType.EndRoom, mainPathLength);
        DungeonUtils.ConnectNodes(current, endRoom);
        return endRoom;
    }
}