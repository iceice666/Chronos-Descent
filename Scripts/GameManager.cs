using Godot;

namespace ChronosDescent.Scripts;

public partial class GameManager : Node
{
    // Weapon selection
    public string SelectedWeapon { get; set; } = "Bow";
    
    // LifeSaving ability selection
    public string SelectedAbility { get; set; } = "Dash";
    
    public override void _Ready()
    {
        // Make this a singleton that persists between scene changes
        ProcessMode = ProcessModeEnum.Always;
    }
}