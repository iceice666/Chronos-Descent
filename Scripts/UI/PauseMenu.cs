using ChronosDescent.Scripts.Core;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class PauseMenu : Control
{
    private bool _isPaused;
    private Button _quitButton;
    private Button _restartButton;
    private Button _resumeButton;

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("Panel/ButtonContainer/ResumeButton");
        _restartButton = GetNode<Button>("Panel/ButtonContainer/RestartButton");
        _quitButton = GetNode<Button>("Panel/ButtonContainer/QuitButton");

        _resumeButton.Pressed += OnResumePressed;
        _restartButton.Pressed += OnRestartPressed;
        _quitButton.Pressed += OnQuitPressed;

        ApplyTranslation();

        // Hide the pause menu initially
        Visible = false;
        _isPaused = false;
    }

    public override void _ExitTree()
    {
        _resumeButton.Pressed -= OnResumePressed;
        _restartButton.Pressed -= OnRestartPressed;
        _quitButton.Pressed -= OnQuitPressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) TogglePause();
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            // Pause the game
            GetTree().Paused = true;
            GameStats.Instance.PauseTimer();
            Visible = true;
        }
        else
        {
            // Resume the game
            GetTree().Paused = false;
            GameStats.Instance.ResumeTimer();
            Visible = false;
        }
    }

    private void OnResumePressed()
    {
        TogglePause();
    }

    private void OnRestartPressed()
    {
        // Reset game stats
        GameStats.Instance.Reset();

        // Unpause the game
        GetTree().Paused = false;

        // Reset and restart the game
        GetTree().ChangeSceneToFile("res://Scenes/prepare_room.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
    
    private void ApplyTranslation()
    {
        var pathKeyPairs = new[]
        {
            (Path: "Panel/TitleLabel", Type: typeof(Label), Key: "Pause_Title"),
            (Path: "Panel/ButtonContainer/ResumeButton", Type: typeof(Button), Key: "UI_Resume"),
            (Path: "Panel/ButtonContainer/RestartButton", Type: typeof(Button), Key: "UI_Restart"),
            (Path: "Panel/ButtonContainer/QuitButton", Type: typeof(Button), Key: "UI_Quit")
        };

        foreach (var pair in pathKeyPairs)
        {
            if (pair.Type == typeof(Label))
            {
                GetNode<Label>(pair.Path).Text = Tr(pair.Key);
            }
            else if (pair.Type == typeof(Button))
            {
                GetNode<Button>(pair.Path).Text = Tr(pair.Key);
            }
        }
    }
}