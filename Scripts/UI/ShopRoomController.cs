using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Items;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Controls the shop room behavior
/// </summary>
[GlobalClass]
public partial class ShopRoomController : Node2D
{
    // Used items to avoid duplication
    private readonly List<string> _usedItems = new();

    // Count of purchased items
    private int _purchasedItems;
    [Export] public PackedScene[] PossibleItems { get; set; }

    // Positions where shop items can be placed
    [Export] public Node2D[] ItemPositions { get; set; }

    // Room cleared when all items are purchased
    [Export] public bool ClearRoomOnPurchase { get; set; }

    public override void _Ready()
    {
        // Subscribe to global events
        GlobalEventBus.Instance.Subscribe<string>(GlobalEventVariant.ShopTransactionCompleted, OnItemPurchased);

        // Find item positions if not set
        if (ItemPositions == null || ItemPositions.Length == 0)
        {
            var positions = GetTree().GetNodesInGroup("ItemPosition").Cast<Node2D>().ToArray();
            if (positions.Length > 0) ItemPositions = positions;
        }
    }

    private void OnItemPurchased(string itemName)
    {
        _purchasedItems++;

        // If all items purchased and room should clear on purchase
        if (ClearRoomOnPurchase && _purchasedItems >= ItemPositions.Length)
            // Signal room is cleared
            GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
    }

    /// <summary>
    ///     Called when the room is entered
    /// </summary>
    public void OnRoomEntered()
    {
        // Populate shop with items
        PopulateShop();
    }

    private void PopulateShop()
    {
        if (PossibleItems == null || PossibleItems.Length == 0 || ItemPositions == null || ItemPositions.Length == 0)
            return;

        var itemsToPlace = Mathf.Min(PossibleItems.Length, ItemPositions.Length);
        var random = new RandomNumberGenerator();
        random.Randomize();

        // Shuffle positions
        var shuffledPositions = ItemPositions.OrderBy(_ => random.RandiRange(0, 100)).ToArray();

        // Place items
        for (var i = 0; i < itemsToPlace; i++)
        {
            // Pick random item
            var itemIndex = random.RandiRange(0, PossibleItems.Length - 1);
            var itemScene = PossibleItems[itemIndex];

            // Skip if already used or null
            if (itemScene == null || _usedItems.Contains(itemScene.ResourcePath)) continue;

            // Instantiate item
            var item = itemScene.Instantiate<ShopItem>();
            AddChild(item);

            // Position item
            item.Position = shuffledPositions[i].Position;

            // Track used items
            _usedItems.Add(itemScene.ResourcePath);
        }
    }

    public override void _ExitTree()
    {
        // Unsubscribe from global events
        GlobalEventBus.Instance.Unsubscribe<string>(GlobalEventVariant.ShopTransactionCompleted, OnItemPurchased);
    }
}