using System;
using System.Collections.Generic;
using Godot;

namespace ChronosDescent.Scripts.Core;

/// <summary>
/// Manages translations for the game.
/// </summary>
[GlobalClass]
public partial class TranslationManager : Node
{
    public static TranslationManager Instance { get; private set; }
    
    // List of available languages and their locale codes
    private readonly Dictionary<string, string> _availableLanguages = new()
    {
        { "English", "en" },
        { "繁體中文", "zh_TW" },
        // Add more languages as needed
    };
    
    // Current language
    private string _currentLanguage = "English";
    
    // Signal for language change
    [Signal]
    public delegate void LanguageChangedEventHandler(string languageCode);
    
    /// <summary>
    /// Gets or sets the current language. Setting this will change the game's language.
    /// </summary>
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value || !_availableLanguages.ContainsKey(value))
                return;
                
            _currentLanguage = value;
            var localeCode = _availableLanguages[value];
            TranslationServer.SetLocale(localeCode);
            
            // Save the language preference
            var config = new ConfigFile();
            config.SetValue("settings", "language", value);
            config.Save("user://settings.cfg");
            
            // Emit signal
            EmitSignal(SignalName.LanguageChanged, localeCode);
        }
    }
    
    /// <summary>
    /// Get a list of available languages
    /// </summary>
    public IReadOnlyCollection<string> AvailableLanguages => _availableLanguages.Keys;
    
    /// <summary>
    /// Get the locale code for a language
    /// </summary>
    public string GetLocaleCode(string language)
    {
        return _availableLanguages.TryGetValue(language, out var code) ? code : "en";
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        // Load language preference from config
        var config = new ConfigFile();
        var err = config.Load("user://settings.cfg");
        
        if (err == Error.Ok && config.HasSectionKey("settings", "language"))
        {
            var savedLanguage = (string)config.GetValue("settings", "language");
            if (_availableLanguages.ContainsKey(savedLanguage))
            {
                _currentLanguage = savedLanguage;
            }
        }
        
        // Set the initial language
        TranslationServer.SetLocale(_availableLanguages[_currentLanguage]);
        
        // Load translations
        LoadTranslations();
    }
    
    /// <summary>
    /// Load all translation files from the Translations directory
    /// </summary>
    private void LoadTranslations()
    {
        // Load translations from .mo or .po files
        // This assumes you've placed your translation files in res://Translations/
        
        foreach (var langCode in _availableLanguages.Values)
        {
            var path = $"res://Translations/{langCode}.po";
            if (ResourceLoader.Exists(path))
            {
                var translation = ResourceLoader.Load<Translation>(path);
                TranslationServer.AddTranslation(translation);
            }
        }
    }
    
    /// <summary>
    /// Translate a string using the current locale
    /// </summary>
    public static string Tr(string text)
    {
        return TranslationServer.Translate(text);
    }
    
    /// <summary>
    /// Translate a string with formatting using the current locale
    /// </summary>
    public static string TrFormat(string text, params object[] args)
    {
        return string.Format(TranslationServer.Translate(text), args);
    }
}
