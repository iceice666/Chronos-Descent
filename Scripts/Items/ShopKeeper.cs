using System.Collections.Generic;
using Godot;

namespace ChronosDescent.Scripts.Items;

/// <summary>
///     Shop keeper NPC for Hades-like shop experience
/// </summary>
[GlobalClass]
public partial class ShopKeeper : Node2D
{
    private readonly List<ShopItem> _spawnedItems = [];
    private Label _dialogueLabel;
    private Panel _dialoguePanel;
    private Timer _dialogueTimer;
    private Area2D _interactionArea;
    private AnimatedSprite2D _shopkeeperSprite;

    [Export]
    public string[] Greetings { get; set; } =
    {
        "Welcome, time traveler. Care to browse my wares?",
        "Ah, a customer from this timeline. How... interesting.",
        "Time is money, and I have plenty of both!",
        "These items are quite rare, from across multiple timelines."
    };

    [Export]
    public string[] Farewells { get; set; } =
    {
        "Until we meet again... perhaps in another timeline.",
        "Time waits for no one. Except me, of course!",
        "May your journeys through time be... profitable.",
        "Come back when you have more chronoshards!"
    };

    [Export]
    public string[] PurchaseResponses { get; set; } =
    {
        "An excellent choice!",
        "Use it wisely, traveler.",
        "That one is my favorite.",
        "A fine purchase!"
    };

    [Export] public PackedScene[] AvailableItems { get; set; }
    [Export] public int MaxItems { get; set; } = 3;
    [Export] public float ItemSpacing { get; set; } = 150.0f;

    public override void _Ready()
    {
        _shopkeeperSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _dialoguePanel = GetNodeOrNull<Panel>("DialoguePanel");
        _dialogueLabel = _dialoguePanel.GetNodeOrNull<Label>("DialogueLabel");
        _dialogueTimer = GetNodeOrNull<Timer>("DialogueTimer");
        _interactionArea = GetNodeOrNull<Area2D>("InteractionArea");

        // Connect signals
        if (_interactionArea != null)
        {
            _interactionArea.BodyEntered += OnPlayerEntered;
            _interactionArea.BodyExited += OnPlayerExited;
        }

        if (_dialogueTimer != null) _dialogueTimer.Timeout += OnDialogueTimerTimeout;

        // Spawn items
        SpawnShopItems();
    }

    private void SpawnShopItems()
    {
        if (AvailableItems == null || AvailableItems.Length == 0 || MaxItems <= 0)
            return;

        // Clear any existing items
        foreach (var item in _spawnedItems) item.QueueFree();

        _spawnedItems.Clear();

        // Calculate item placement
        var startX = -(ItemSpacing * (MaxItems - 1)) / 2;

        // Choose random items to spawn
        var random = new RandomNumberGenerator();
        random.Randomize();

        for (var i = 0; i < MaxItems; i++)
        {
            // Pick random item from available items
            var itemIndex = random.RandiRange(0, AvailableItems.Length - 1);
            var itemScene = AvailableItems[itemIndex];

            if (itemScene == null) continue;

            // Instantiate the item
            var shopItem = itemScene.Instantiate<ShopItem>();
            AddChild(shopItem);

            // Position the item
            shopItem.GlobalPosition =
                new Vector2(startX + i * ItemSpacing, 0);

            // Connect to purchase signal
            shopItem.ItemPurchased += OnItemPurchased;

            // Track spawned items
            _spawnedItems.Add(shopItem);
        }
    }

    private void OnPlayerEntered(Node2D body)
    {
        // Play greeting animation
        if (_shopkeeperSprite != null && _shopkeeperSprite.SpriteFrames.HasAnimation("greet"))
            _shopkeeperSprite.Play("greet");

        // Show random greeting dialogue
        if (Greetings.Length > 0)
        {
            var random = new RandomNumberGenerator();
            random.Randomize();
            var greeting = Greetings[random.RandiRange(0, Greetings.Length - 1)];
            ShowDialogue(greeting);
        }
    }

    private void OnPlayerExited(Node2D body)
    {
        // Play farewell animation
        if (_shopkeeperSprite != null && _shopkeeperSprite.SpriteFrames.HasAnimation("farewell"))
            _shopkeeperSprite.Play("farewell");

        // Show random farewell dialogue
        if (Farewells.Length > 0)
        {
            var random = new RandomNumberGenerator();
            random.Randomize();
            var farewell = Farewells[random.RandiRange(0, Farewells.Length - 1)];
            ShowDialogue(farewell);
        }
    }

    private void OnItemPurchased(string itemName)
    {
        // Play purchase reaction animation
        if (_shopkeeperSprite != null && _shopkeeperSprite.SpriteFrames.HasAnimation("react"))
            _shopkeeperSprite.Play("react");

        // Show random purchase response dialogue
        if (PurchaseResponses.Length > 0)
        {
            var random = new RandomNumberGenerator();
            random.Randomize();
            var response = PurchaseResponses[random.RandiRange(0, PurchaseResponses.Length - 1)];
            ShowDialogue(response);
        }
    }

    private void ShowDialogue(string text)
    {
        if (_dialogueLabel == null) return;

        _dialogueLabel.Text = text;
        _dialoguePanel?.Show();


        // Start dialogue timer
        _dialogueTimer?.Start();
    }

    private void OnDialogueTimerTimeout()
    {
        _dialoguePanel?.Hide();
    }
}