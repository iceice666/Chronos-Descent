using ChronosDescent.Scripts.Core;
using Godot;
using System.Linq;

namespace ChronosDescent.Scripts.UI;

[GlobalClass]
public partial class LanguageSelector : Control
{
    private OptionButton _languageDropdown;
    
    public override void _Ready()
    {
        _languageDropdown = GetNode<OptionButton>("LanguageDropdown");
        
        // Initialize language dropdown
        InitializeLanguageDropdown();
        
        // Connect signal
        _languageDropdown.ItemSelected += OnLanguageSelected;
    }
    
    public override void _ExitTree()
    {
        _languageDropdown.ItemSelected -= OnLanguageSelected;
    }
    
    private void InitializeLanguageDropdown()
    {
        _languageDropdown.Clear();
        
        // Get available languages from TranslationManager
        var languages = TranslationManager.Instance.AvailableLanguages.ToArray();
        
        // Add languages to dropdown
        for (var i = 0; i < languages.Length; i++)
        {
            _languageDropdown.AddItem(languages[i], i);
            
            // Set current language as selected
            if (languages[i] == TranslationManager.Instance.CurrentLanguage)
            {
                _languageDropdown.Selected = i;
            }
        }
    }
    
    private void OnLanguageSelected(long index)
    {
        // Get selected language
        var language = _languageDropdown.GetItemText((int)index);
        
        // Set language in TranslationManager
        TranslationManager.Instance.CurrentLanguage = language;
    }
}