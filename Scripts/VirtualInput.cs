using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts;

public partial class VirtualInput : Control
{
    private UserInputManager _inputManager;
    private Label _sourceIndicator;

    public override void _Ready()
    {
        _sourceIndicator = GetNode<Label>("../SourceIndicator");
        _inputManager = GetNode<UserInputManager>("/root/Autoload/UserInputManager");
        _inputManager.InputSourceChanged += OnSourceChanged;

        _sourceIndicator.Text = _inputManager.CurrentInputSource.ToString();
    }

    public override void _ExitTree()
    {
        _inputManager.InputSourceChanged -= OnSourceChanged;
    }

    private void OnSourceChanged(UserInputManager.InputSource newSource)
    {
        _sourceIndicator.Text = newSource.ToString();
    }
}