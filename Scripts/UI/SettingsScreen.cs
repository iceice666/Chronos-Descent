using ChronosDescent.Scripts.Core;
using Godot;

namespace ChronosDescent.Scripts.UI;

[GlobalClass]
public partial class SettingsScreen : Control
{
    [Signal]
    public delegate void SettingsClosedEventHandler();

    private Button _applyButton;
    private Button _closeButton;
    private string _currentLanguage;

    private bool _fullscreen;

    // UI Elements
    private OptionButton _languageDropdown;

    // Settings values
    private float _musicVolume = 1.0f;
    private HSlider _musicVolumeSlider;
    private float _sfxVolume = 1.0f;
    private HSlider _sfxVolumeSlider;
    private OptionButton _windowMode;

    public override void _Ready()
    {
        // Get UI element references
        _languageDropdown = GetNode<OptionButton>("%LanguageDropdown");
        _musicVolumeSlider = GetNode<HSlider>("%MusicVolumeSlider");
        _sfxVolumeSlider = GetNode<HSlider>("%SFXVolumeSlider");
        _windowMode = GetNode<OptionButton>("%WindowMode");
        _closeButton = GetNode<Button>("%CloseButton");
        _applyButton = GetNode<Button>("%ApplyButton");

        // Set up language dropdown
        InitializeLanguageDropdown();

        // Connect signals
        _languageDropdown.ItemSelected += OnLanguageSelected;
        _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
        _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
        _windowMode.ItemSelected += OnWindowModeSelected;
        _closeButton.Pressed += OnClosePressed;
        _applyButton.Pressed += OnApplyPressed;


        // Load current settings
        LoadSettings();

        // Apply translations to UI elements
        ApplyTranslations();
    }

    public override void _ExitTree()
    {
        // Disconnect signals
        _languageDropdown.ItemSelected -= OnLanguageSelected;
        _musicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
        _sfxVolumeSlider.ValueChanged -= OnSFXVolumeChanged;
        _windowMode.ItemSelected -= OnWindowModeSelected;
        _closeButton.Pressed -= OnClosePressed;
        _applyButton.Pressed -= OnApplyPressed;
    }

    private void InitializeLanguageDropdown()
    {
        _languageDropdown.Clear();

        // Get available languages from TranslationManager
        var languages = TranslationManager.Instance.AvailableLanguages;
        var index = 0;
        var selectedIndex = 0;

        foreach (var language in languages)
        {
            _languageDropdown.AddItem(language, index);

            // Set current language as selected
            if (language == TranslationManager.Instance.CurrentLanguage) selectedIndex = index;

            index++;
        }

        _languageDropdown.Selected = selectedIndex;
        _currentLanguage = TranslationManager.Instance.CurrentLanguage;
    }

    private void LoadSettings()
    {
        // Load audio settings
        _musicVolume = GameSettings.GetMusicVolume();
        _sfxVolume = GameSettings.GetSFXVolume();
        _musicVolumeSlider.Value = _musicVolume;
        _sfxVolumeSlider.Value = _sfxVolume;

        // Load video settings
        _fullscreen = GameSettings.GetFullscreen();
        _windowMode.ButtonPressed = _fullscreen;
    }

    private void ApplyTranslations()
    {
        // Apply translations to all UI elements
        GetNode<Label>("%LanguageLabel").SetTextTr("Settings_Language");
        GetNode<Label>("%MusicLabel").SetTextTr("Settings_MusicVolume");
        GetNode<Label>("%SFXLabel").SetTextTr("Settings_SfxVolume");
        GetNode<Label>("%WindowModeLabel").SetTextTr("Settings_WindowMode");
        GetNode<Button>("%ApplyButton").SetTextTr("Settings_Apply");
        GetNode<Button>("%ResetButton").SetTextTr("Settings_Reset");

        // Translate tab titles
        var tabContainer = GetNode<TabContainer>("Panel/TabContainer");
        tabContainer.SetTabTitle(0, TranslationManager.Tr("Settings_General"));
        tabContainer.SetTabTitle(1, TranslationManager.Tr("Settings_Audio"));
        tabContainer.SetTabTitle(2, TranslationManager.Tr("Settings_Video"));

        // Translate window mode options
        _windowMode.SetItemText(0, TranslationManager.Tr("Settings_WindowMode_Windowed"));
        _windowMode.SetItemText(1, TranslationManager.Tr("Settings_WindowMode_Fullscreen"));
        _windowMode.SetItemText(2, TranslationManager.Tr("Settings_WindowMode_Maximized"));
    }

    private void OnLanguageSelected(long index)
    {
        _currentLanguage = _languageDropdown.GetItemText((int)index);
    }

    private void OnMusicVolumeChanged(double value)
    {
        _musicVolume = (float)value;
    }

    private void OnSFXVolumeChanged(double value)
    {
        _sfxVolume = (float)value;
    }

    private static void OnWindowModeSelected(long index)
    {
        switch (index)
        {
            case 0:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                break;
            case 1:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                break;
            case 2:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
                break;
        }
    }

    private void OnClosePressed()
    {
        EmitSignal(SignalName.SettingsClosed);
        Hide();
    }

    private void OnApplyPressed()
    {
        // Apply settings
        if (_currentLanguage != TranslationManager.Instance.CurrentLanguage)
        {
            TranslationManager.Instance.CurrentLanguage = _currentLanguage;
            // Update translations after changing language
            ApplyTranslations();
        }

        // Apply audio settings
        GameSettings.SetMusicVolume(_musicVolume);
        GameSettings.SetSFXVolume(_sfxVolume);

        // Apply video settings
        GameSettings.SetFullscreen(_fullscreen);
        DisplayServer.WindowSetMode(_fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);

        // Save settings
        GameSettings.SaveSettings();
    }
}