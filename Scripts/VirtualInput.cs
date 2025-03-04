using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts;

public partial class VirtualInput : Container
{
    private node.UserInputManager _userInputManager;

    public override void _Ready()
    {
        _userInputManager = GetNode<node.UserInputManager>("/root/Autoload/UserInputManager");
        _userInputManager.InputSourceChanged += OnInputSourceChange;
    }

    private void OnInputSourceChange(node.UserInputManager.InputSource newSource)
    {
        Visible = newSource == node.UserInputManager.InputSource.VirtualJoystick;
    }


}