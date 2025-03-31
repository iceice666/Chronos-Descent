using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

[GlobalClass]
public partial class RoomRegistryResource : Resource
{
    [Export] public PackedScene[] StartRooms { get; set; }
    [Export] public PackedScene[] EndRooms { get; set; }
    [Export] public PackedScene[] BossRooms { get; set; }
    [Export] public PackedScene[] MiniBossRooms { get; set; }
    [Export] public PackedScene[] CombatRooms { get; set; }
    [Export] public PackedScene[] RewardRooms { get; set; }
    [Export] public PackedScene[] EventRooms { get; set; }
    [Export] public PackedScene[] ShopRooms { get; set; }
}