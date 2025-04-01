using System;
using ChronosDescent.Scripts.Core.Currency;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Items;

/// <summary>
///     Physical shop item that can be purchased via interaction
/// </summary>
[GlobalClass]
public partial class ShopItem : Node2D
{
    // Signals
    [Signal]
    public delegate void ItemPurchasedEventHandler(string itemName);

    private Label _costLabel;
    private Area2D _interactionArea;
    private Sprite2D _itemSprite;

    // Visual components
    private Label _nameLabel;
    private Vector2 _originalPosition;
    private bool _purchased;
    private float _time;

    // State tracking
    protected Player PlayerInRange;
    [Export] public string ItemName { get; set; } = "Unknown Item";
    [Export] public string ItemDescription { get; set; } = "No description available.";
    [Export] public int Cost { get; set; } = 100;
    [Export] public Texture2D ItemIcon { get; set; }
    [Export] public float HoverDistance { get; set; } = 5.0f;
    [Export] public float HoverSpeed { get; set; } = 3.0f;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("NameLabel");
        _costLabel = GetNode<Label>("CostLabel");
        _itemSprite = GetNode<Sprite2D>("ItemSprite");
        _interactionArea = GetNode<Area2D>("InteractionArea");

        // Store original position for hover effect
        _originalPosition = GlobalPosition with { Y = 100 };

        // Set up visual elements
        if (_nameLabel != null) _nameLabel.Text = ItemName;
        if (_costLabel != null) _costLabel.Text = Cost.ToString();
        if (_itemSprite != null && ItemIcon != null) _itemSprite.Texture = ItemIcon;

        // Connect signals
        _interactionArea.BodyEntered += OnBodyEntered;
        _interactionArea.BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        // Apply hovering effect
        _time += (float)delta * HoverSpeed;
        Position = _originalPosition + new Vector2(0, MathF.Sin(_time) * HoverDistance);

        // Check for interaction key press when player is in range
        if (PlayerInRange != null && !_purchased && Input.IsActionJustPressed("interact")) TryPurchase();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player && !_purchased)
        {
            PlayerInRange = player;

            // Show "can interact" indicator
            ShowInteractionPrompt(true);


            // Update UI to show if player can afford it
            UpdateAffordabilityDisplay(player.CurrencyManager.Chronoshards >= Cost);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player && PlayerInRange != null)
        {
            PlayerInRange = null;

            // Hide interaction prompt
            ShowInteractionPrompt(false);
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        var prompt = GetNodeOrNull<Node2D>("%InteractPrompt");
        if (prompt != null) prompt.Visible = show;
    }

    private void UpdateAffordabilityDisplay(bool canAfford)
    {
        if (_costLabel != null)
            _costLabel.Modulate = canAfford ? new Color(0.2f, 1.0f, 0.2f) : new Color(1.0f, 0.2f, 0.2f);
    }

    private void TryPurchase()
    {
        if (PlayerInRange == null || _purchased) return;

        var result = Transaction.TryPurchase(PlayerInRange, Cost);

        switch (result)
        {
            case Transaction.Result.Success:
            {
                // Mark as purchased
                _purchased = true;

                // Hide interaction elements
                ShowInteractionPrompt(false);
                if (_costLabel != null) _costLabel.Visible = false;


                // Apply the item effect
                ApplyItemEffect();

                // Emit signal for any listeners
                EmitSignal(SignalName.ItemPurchased, ItemName);
                break;
            }
            case Transaction.Result.InsufficientFunds:
                // Show "cannot afford" feedback
                break;
        }
    }

    protected virtual void ApplyItemEffect()
    {
        // Override in derived classes to apply specific effects
        // Base implementation does nothing
    }
}