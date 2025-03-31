using System;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

/// <summary>
/// Types of rooms that can be placed in the dungeon
/// </summary>
public enum RoomType
{
    StartRoom,
    EndRoom,
    BossRoom,
    MiniBossRoom,
    CombatRoom,
    RewardRoom,
    EventRoom,
    ShopRoom,
}

public static class RoomTypeExtensions
{
    public static string GetSymbol(this RoomType roomType)
    {
        return roomType switch
        {
            RoomType.StartRoom => "St",
            RoomType.EndRoom => "Ed",
            RoomType.BossRoom => "Bo",
            RoomType.MiniBossRoom => "bo",
            RoomType.CombatRoom => "Co",
            RoomType.RewardRoom => "Rd",
            RoomType.EventRoom => "Ev",
            RoomType.ShopRoom => "Sp",
            _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
        };
    }
}

/// <summary>
/// Represents a room type with its scene
/// </summary>
[GlobalClass]
public partial class RoomUnit : Resource
{
    [Export] public RoomType Type { get; set; }
    [Export] public PackedScene Scene { get; set; }
}