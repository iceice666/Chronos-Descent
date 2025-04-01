using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class TitleScreenButton : VBoxContainer
{
    private Button _newRunButton;
    private Button _quitButton;
    private Button _langButton;

    public override void _Ready()
    {
        _newRunButton = GetNode<Button>("NewRunButton");
        _quitButton = GetNode<Button>("QuitButton");
        _langButton = GetNode<Button>("LangButton");


        _quitButton.Pressed += () => GetTree().Quit();
    }
}