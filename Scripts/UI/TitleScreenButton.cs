using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class TitleScreenButton : VBoxContainer
{
    private Button _langButton;
    private Button _newRunButton;
    private Button _quitButton;

    public override void _Ready()
    {
        _newRunButton = GetNode<Button>("NewRunButton");
        _quitButton = GetNode<Button>("QuitButton");
        _langButton = GetNode<Button>("LangButton");

        _newRunButton.Pressed += OnNewRunPressed;
        _quitButton.Pressed += () => GetTree().Quit();
    }

    private void OnNewRunPressed()
    {
        // Load the preparation room scene
        GetTree().ChangeSceneToFile("res://Scenes/prepare_room.tscn");
    }
}