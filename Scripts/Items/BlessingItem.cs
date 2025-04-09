using System;
using ChronosDescent.Scripts.ActionManager;
using ChronosDescent.Scripts.Core.Blessing;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Items;

/// <summary>
///     Physical blessing item that can be received via interaction
/// </summary>
[GlobalClass]
public partial class BlessingItem : Node2D
{
    private Label _categoryLabel;
    private Label _descriptionLabel;

    // The color to use for the blessing's glow effect
    private Color _glowColor = Colors.White;
    private TextureRect _iconSprite;
    private Area2D _interactionArea;

    // Visual components
    private Label _nameLabel;
    private Vector2 _originalPosition;
    private float _time;
    private Node2D _promptNode;

    // State tracking
    protected Player PlayerInRange;
    [Export] public Blessing Blessing { get; set; }
    [Export] public float HoverDistance { get; set; } = 5.0f;
    [Export] public float HoverSpeed { get; set; } = 3.0f;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("NameLabel");
        _descriptionLabel = GetNode<Label>("DescriptionLabel");
        _categoryLabel = GetNode<Label>("CategoryLabel");
        _iconSprite = GetNode<TextureRect>("IconSprite");
        _interactionArea = GetNode<Area2D>("InteractionArea");

        // Store original position for hover effect
        _originalPosition = GlobalPosition;

        // Setup blessing info if available
        if (Blessing != null) SetupBlessingVisuals();

        // Connect signals
        _interactionArea.AreaEntered += OnBodyEntered;
        _interactionArea.AreaExited += OnBodyExited;
        GlobalEventBus.Instance.Subscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
        GlobalEventBus.Instance.Subscribe<UserInputManager.InputSource>(GlobalEventVariant.InputSourceChanged, UpdatePromptNode);


        UpdatePromptNode(UserInputManager.Instance.CurrentInputSource);
    }

    public override void _ExitTree()
    {
        _interactionArea.AreaEntered -= OnBodyEntered;
        _interactionArea.AreaExited -= OnBodyExited;
        GlobalEventBus.Instance.Unsubscribe<Blessing>(GlobalEventVariant.BlessingSelected, OnBlessingSelected);
        GlobalEventBus.Instance.Unsubscribe<UserInputManager.InputSource>(GlobalEventVariant.InputSourceChanged, UpdatePromptNode);
    }
    
    private void UpdatePromptNode(UserInputManager.InputSource src)  {
        var prevState =_promptNode is { Visible: true };
            
        switch (src)
        {
            case UserInputManager.InputSource.KeyboardMouse:
            case UserInputManager.InputSource.Controller:
                _promptNode = GetNodeOrNull<Node2D>("%InteractPrompt");

                var actionName = InputMap.ActionGetEvents("interact");


                foreach (var ie in actionName)
                {
                    if (UserInputManager.Instance.CurrentInputSource == UserInputManager.InputSource.Controller &&
                        ie is InputEventJoypadButton e)
                    {
                        _promptNode.GetNode<Label>("Panel/Label").Text = e.GetButtonIndex().ToString();
                        break;
                    }

                    if (UserInputManager.Instance.CurrentInputSource == UserInputManager.InputSource.KeyboardMouse &&
                        ie is InputEventKey ee)
                    {
                        _promptNode.GetNode<Label>("Panel/Label").Text = ee.PhysicalKeycode.ToString();
                        break;
                    }
                }


                break;
           
            case UserInputManager.InputSource.VirtualJoystick:
                _promptNode = GetNode<Node2D>("/root/Dungeon/UI/VirtualInput/InteractButtonNode");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(src), src, null);
        }

        _promptNode.Visible = prevState;
    }

    public void SetBlessing(Blessing blessing)
    {
        Blessing = blessing;
        SetupBlessingVisuals();
    }

    private void SetupBlessingVisuals()
    {
        if (_nameLabel != null) _nameLabel.Text = Blessing.Title;
        if (_descriptionLabel != null) _descriptionLabel.Text = Blessing.Description;
        if (_categoryLabel != null) _categoryLabel.Text = Blessing.Category.ToString();

        // Set icon if available
        if (_iconSprite != null && Blessing.Icon != null)
        {
            _iconSprite.Texture = Blessing.Icon;
        }
        else if (_iconSprite != null)
        {
            // Use colored icon based on category
            switch (Blessing.Category)
            {
                case BlessingCategory.Offensive:
                    _glowColor = new Color(1.0f, 0.3f, 0.3f); // Red
                    break;
                case BlessingCategory.Defensive:
                    _glowColor = new Color(0.3f, 0.7f, 1.0f); // Blue
                    break;
                case BlessingCategory.Utility:
                    _glowColor = new Color(1.0f, 0.8f, 0.2f); // Yellow
                    break;
                case BlessingCategory.Movement:
                    _glowColor = new Color(0.2f, 0.9f, 0.5f); // Green
                    break;
            }

            _iconSprite.Modulate = _glowColor;
        }

        // Set nameLabel color based on rarity
        if (_nameLabel != null) _nameLabel.Modulate = Blessing.GetRarityColor();
    }

    public override void _Process(double delta)
    {
        // Apply hovering effect
        _time += (float)delta * HoverSpeed;
        GlobalPosition = _originalPosition + new Vector2(0, Mathf.Sin(_time) * HoverDistance);

        // Check for interaction key press when player is in range
        if (Input.IsActionJustPressed("interact") && PlayerInRange != null) CollectBlessing();
    }

    private void OnBodyEntered(Area2D area)
    {
        if (area is not Hurtbox) return; 
        PlayerInRange = (Player) area.Owner;

        // Show "can interact" indicator
        ShowInteractionPrompt(true);
    }

    private void OnBodyExited(Area2D area)
    {
        if (area is Hurtbox && PlayerInRange != null)
        {
            PlayerInRange = null;

            // Hide interaction prompt
            ShowInteractionPrompt(false);
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        if (_promptNode == null) return;
            _promptNode.Visible = show;
            Player.Instance.StateLabel.Text = show.ToString();
    }

    private void CollectBlessing()
    {
        if (PlayerInRange == null || Blessing == null) return;

        // Hide interaction elements
        ShowInteractionPrompt(false);

        // Apply blessing effect
        PlayerInRange.BlessingManager.AddBlessing(Blessing);

        // Create visual effect
        CreateBlessingEffect();

        // Publish global event
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingSelected, Blessing);
    }

    private void CreateBlessingEffect()
    {
        // Create particles for visual feedback
        var particles = new GpuParticles2D();

        // Set up particles material
        var material = new ParticleProcessMaterial();
        material.Color = _glowColor;

        // Configure particles
        particles.ProcessMaterial = material;
        particles.Emitting = true;
        particles.OneShot = true;
        particles.Amount = 16;
        particles.Lifetime = 1.0f;

        // Add to scene
        AddChild(particles);
    }

    private void OnBlessingSelected(Blessing _)
    {
        QueueFree();
    }
}