using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts;

public partial class VirtualInput : Container
{
    public override void _Ready()
    {
        var manager = GetNode<UserInputManager>("/root/Autoload/UserInputManager");
        manager.InputSourceChanged += OnInputSourceChange;
    }

    private void OnInputSourceChange(UserInputManager.InputSource newSource)
    {
        Visible = newSource == UserInputManager.InputSource.VirtualJoystick;
    }
}