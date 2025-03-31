using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

/// <summary>
/// Resource for creating a RoomUnit in the editor
/// </summary>
[GlobalClass]
public partial class RoomUnitResource : Resource
{
    [Export] public RoomType Type { get; set; }
    [Export(PropertyHint.File, "*.tscn")] public string ScenePath { get; set; }

    /// <summary>
    /// Converts this resource to a RoomUnit
    /// </summary>
    public RoomUnit ToRoomUnit()
    {
        return new RoomUnit
        {
            Type = Type,
            Scene = GD.Load<PackedScene>(ScenePath)
        };
    }
}