using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts;

public partial class VirtualInput : Control
{
    private UserInputManager _inputManager;
    private Label _sourceIndicator;

    public override void _Ready()
    {
        _inputManager = GetNode<UserInputManager>("/root/Autoload/UserInputManager");
    }

    public override void _ExitTree()
    {
    }

  
}