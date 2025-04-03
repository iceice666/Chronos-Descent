using Godot;
using System;

namespace ChronosDescent.Scripts.Core;

/// <summary>
/// Extension methods for various Godot classes to simplify i18n usage
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Set the text of a Label with translation
    /// </summary>
    public static void SetTextTr(this Label label, string key)
    {
        label.Text = TranslationManager.Tr(key);
    }
    
    /// <summary>
    /// Set the text of a Label with translation and formatting
    /// </summary>
    public static void SetTextTrFormat(this Label label, string key, params object[] args)
    {
        label.Text = TranslationManager.TrFormat(key, args);
    }
    
    /// <summary>
    /// Set the text of a Button with translation
    /// </summary>
    public static void SetTextTr(this Button button, string key)
    {
        button.Text = TranslationManager.Tr(key);
    }
    
    /// <summary>
    /// Set the tooltip of a Control with translation
    /// </summary>
    public static void SetTooltipTextTr(this Control control, string key)
    {
        control.TooltipText = TranslationManager.Tr(key);
    }
    
    /// <summary>
    /// Set the tooltip of a Control with translation and formatting
    /// </summary>
    public static void SetTooltipTextTrFormat(this Control control, string key, params object[] args)
    {
        control.TooltipText = TranslationManager.TrFormat(key, args);
    }
    
    /// <summary>
    /// Set an item's text in an OptionButton with translation
    /// </summary>
    public static void SetItemTextTr(this OptionButton optionButton, int index, string key)
    {
        optionButton.SetItemText(index, TranslationManager.Tr(key));
    }
}