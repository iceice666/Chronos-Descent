using ChronosDescent.Scripts.Core.Currency;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Button representing a purchasable item in a shop
/// </summary>
[GlobalClass]
public partial class ShopItemButton : Button
{
    // Signals
    [Signal]
    public delegate void ItemPurchasedEventHandler(string itemName);

    private Label _costLabel;
    private Label _descriptionLabel;
    private TextureRect _iconTexture;

    private Label _nameLabel;
    private Player _player;
    [Export] public string ItemName { get; set; } = "Unknown Item";
    [Export] public string ItemDescription { get; set; } = "No description available.";
    [Export] public int Cost { get; set; } = 100;
    [Export] public Texture2D ItemIcon { get; set; }

    public override void _Ready()
    {
        // Get UI elements
        _nameLabel = GetNode<Label>("%NameLabel");
        _costLabel = GetNode<Label>("%CostLabel");
        _iconTexture = GetNode<TextureRect>("%ItemIcon");
        _descriptionLabel = GetNode<Label>("%DescriptionLabel");

        // Find the player
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;

        // Initialize UI
        UpdateUI();

        // Connect signals
        Pressed += OnPressed;

        // If player exists, subscribe to currency change events
        if (_player != null)
        {
            _player.CurrencyManager.OnChronoshardsChanged += UpdateButtonState;
            UpdateButtonState(_player.CurrencyManager.Chronoshards);
        }
    }

    public override void _ExitTree()
    {
        // Disconnect signals
        if (_player != null) _player.CurrencyManager.OnChronoshardsChanged -= UpdateButtonState;
    }

    private void UpdateUI()
    {
        if (_nameLabel != null) _nameLabel.Text = ItemName;
        if (_costLabel != null) _costLabel.Text = Cost.ToString();
        if (_iconTexture != null && ItemIcon != null) _iconTexture.Texture = ItemIcon;
        if (_descriptionLabel != null) _descriptionLabel.Text = ItemDescription;
    }

    private void UpdateButtonState(int currentCurrency)
    {
        // Disable button if player can't afford the item
        Disabled = currentCurrency < Cost;
    }

    private void OnPressed()
    {
        if (_player == null) return;

        // Attempt to purchase the item
        var result = Transaction.TryPurchase(_player, Cost);

        if (result == Transaction.Result.Success)
            // Emit signal that item was purchased
            EmitSignal(SignalName.ItemPurchased, ItemName);
        // Play purchase sound or animation effect
        // TODO: Add effects
    }

    /// <summary>
    ///     Sets all the item properties at once
    /// </summary>
    public void SetItemData(string name, string description, int cost, Texture2D icon)
    {
        ItemName = name;
        ItemDescription = description;
        Cost = cost;
        ItemIcon = icon;

        UpdateUI();

        if (_player != null) UpdateButtonState(_player.CurrencyManager.Chronoshards);
    }
}