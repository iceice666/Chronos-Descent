using Godot;
using ChronosDescent.Scripts.Core;

namespace ChronosDescent.Scripts.UI;

public partial class TitleScreenButton : VBoxContainer
{
    private Button _settingsButton;
    private Button _newRunButton;
    private Button _quitButton;

    private PackedScene _settingsScreenScene;
    private Control _settingsScreen;

    public override void _Ready()
    {
        _newRunButton = GetNode<Button>("NewRunButton");
        _quitButton = GetNode<Button>("QuitButton");
        _settingsButton = GetNode<Button>("SettingsButton");

        // Load the settings screen scene
        _settingsScreenScene = GD.Load<PackedScene>("res://Scenes/ui/settings_screen.tscn");

        _newRunButton.Pressed += OnNewRunPressed;
        _quitButton.Pressed += OnQuitPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;

        // Apply translations to button texts
        _newRunButton.SetTextTr("Title_NewRun");
        _quitButton.SetTextTr("Title_Quit");
        _settingsButton.SetTextTr("Title_Settings");
    }


    public override void _ExitTree()
    {
        _newRunButton.Pressed -= OnNewRunPressed;
        _quitButton.Pressed -= OnQuitPressed;
        _settingsButton.Pressed -= OnSettingsButtonPressed;

        // Free the settings screen if it exists
        _settingsScreen?.QueueFree();
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void OnNewRunPressed()
    {
        // Load the preparation room scene
        GetTree().ChangeSceneToFile("res://Scenes/ui/prepare_menu.tscn");
    }

    private void OnSettingsButtonPressed()
    {
        // Toggle settings screen visibility
        if (_settingsScreen == null)
        {
            // If not already created, instantiate the settings screen
            _settingsScreen = _settingsScreenScene.Instantiate<Control>();

            // Add it to the scene, making it fill the entire viewport
            var root = GetTree().Root;
            root.AddChild(_settingsScreen);

            // Connect to the settings closed signal
            if (_settingsScreen is SettingsScreen settingsScreenScript)
            {
                settingsScreenScript.SettingsClosed += OnSettingsClosed;
            }

            // Add close button functionality directly
            var closeButton = _settingsScreen.GetNode<Button>("%CloseButton");
            if (closeButton != null)
            {
                closeButton.Pressed += OnSettingsClosed;
            }
        }
        else
        {
            // If already visible, hide and destroy it
            CloseSettingsScreen();
        }
    }

    private void OnSettingsClosed()
    {
        CloseSettingsScreen();
    }

    private void CloseSettingsScreen()
    {
        if (_settingsScreen == null) return;

        // Disconnect signals
        if (_settingsScreen is SettingsScreen settingsScreenScript)
        {
            settingsScreenScript.SettingsClosed -= OnSettingsClosed;
        }

        var closeButton = _settingsScreen.GetNode<Button>("%CloseButton");
        if (closeButton != null)
        {
            closeButton.Pressed -= OnSettingsClosed;
        }

        // Hide and destroy the settings screen
        _settingsScreen.QueueFree();
        _settingsScreen = null;
    }
}