using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class PauseBtn : Button
{
    public override void _Ready()
    {
        Pressed += () => GetNode<PauseMenu>("/root/Dungeon/UI/PauseMenu").TogglePause(true);
    }
}