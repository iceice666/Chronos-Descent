using ChronosDescent.Scripts.Core.Currency;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Displays the player's currency value
/// </summary>
public partial class CurrencyDisplay : HBoxContainer
{
    private Label _currencyLabel;
    private CurrencyManager _currencyManager;

    public override void _Ready()
    {
        _currencyLabel = GetNode<Label>("CurrencyValue");

        // Hide initially until initialized
        Visible = false;
    }

    /// <summary>
    ///     Initialize with the player's currency manager
    /// </summary>
    public void Initialize(Player player)
    {
        _currencyManager = player.CurrencyManager;

        // Update initial value
        UpdateCurrencyDisplay(_currencyManager.Chronoshards);

        // Subscribe to currency change events
        _currencyManager.OnChronoshardsChanged += UpdateCurrencyDisplay;

        // Show the display
        Visible = true;
    }

    public override void _ExitTree()
    {
        if (_currencyManager != null) _currencyManager.OnChronoshardsChanged -= UpdateCurrencyDisplay;
    }

    /// <summary>
    ///     Update the currency display with the new amount
    /// </summary>
    private void UpdateCurrencyDisplay(int amount)
    {
        _currencyLabel.Text = amount.ToString();
    }
}