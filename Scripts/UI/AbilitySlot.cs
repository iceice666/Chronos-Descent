using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AbilitySlot : Panel
{
    // Animation
    private AnimationPlayer _animationPlayer;
    private ColorRect _cooldownOverlay;
    private Label _cooldownText;
    private Ability _currentAbility;
    private Ability.AbilityState _currentState = Ability.AbilityState.Default;

    private Label _hotKeyLabel;

    // UI components
    private TextureRect _iconNode;
    private Label _nameLabel;

    // Slot properties
    public AbilityManagerComponent.Slot SlotType { get; set; }

    public override void _Ready()
    {
        // Get references to UI components
        _iconNode = GetNode<TextureRect>("Icon");
        _hotKeyLabel = GetNode<Label>("HotKey");
        _nameLabel = GetNode<Label>("Name");
        _cooldownOverlay = GetNode<ColorRect>("CooldownOverlay");
        _cooldownText = GetNode<Label>("CooldownText");

        // Initialize with empty state
        _cooldownOverlay.Visible = false;

        // Add animation player (optional)
        _animationPlayer = new AnimationPlayer
        {
            Name = "AnimationPlayer"
        };
        AddChild(_animationPlayer);

        // Create animations
        CreateAnimations();
    }

    private void CreateAnimations()
    {
        // Optional: Create animations for different states
        // (You can add animations for activation, charging, etc.)
    }

    public void UpdateHotKeyLabel()
    {
        var inputMapKey = SlotType switch
        {
            AbilityManagerComponent.Slot.NormalAttack => "normal_attack",
            AbilityManagerComponent.Slot.Primary => "ability_1",
            AbilityManagerComponent.Slot.Secondary => "ability_2",
            AbilityManagerComponent.Slot.WeaponUlt => "weapon_ult",
            _ => null
        };

        if (inputMapKey == null) return;

        var actions = InputMap.ActionGetEvents(inputMapKey);

        foreach (var @event in actions)
        {
            if (@event is InputEventMouseButton mouseButton && UserInputManager.Instance.CurrentInputSource ==
                UserInputManager.InputSource.KeyboardMouse)
            {
                _hotKeyLabel.Text = mouseButton.ButtonIndex.ToString();
                break;
            }

            if (@event is InputEventKey key)
            {
                _hotKeyLabel.Text = key.PhysicalKeycode.ToString();
                break;
            }

            if (@event is InputEventJoypadButton joypadButton && UserInputManager.Instance.CurrentInputSource ==
                UserInputManager.InputSource.Controller)
            {
                _hotKeyLabel.Text = joypadButton.ButtonIndex.ToString();
                break;
            }
        }
    }

    public void UpdateAbility(Ability ability)
    {
        _currentAbility = ability;

        if (ability == null)
        {
            // Empty slot
            _iconNode.Texture = null;
            _nameLabel.Text = "";
            _cooldownOverlay.Visible = false;
            return;
        }

        // Update with ability info
        _iconNode.Texture = ability.Icon;
        _nameLabel.Text = ability.Name;

        // Reset state
        UpdateState(Ability.AbilityState.Default);
    }

    public void UpdateCooldown(double currentCooldown, double maxCooldown)
    {
        if (_currentAbility == null) return;

        if (currentCooldown <= 0)
        {
            // Ability is ready
            _cooldownOverlay.Visible = false;
            _cooldownText.Text = "";
        }
        else
        {
            // Ability is on cooldown
            _cooldownOverlay.Visible = true;

            // Update cooldown overlay (from 0.0 to 1.0 height ratio)
            var cooldownRatio = (float)(currentCooldown / maxCooldown);
            _cooldownOverlay.Size = new Vector2(_cooldownOverlay.Size.X, cooldownRatio * GetRect().Size.Y);

            // Position cooldown overlay from bottom to top
            _cooldownOverlay.Position = new Vector2(0, GetRect().Size.Y * (1 - cooldownRatio));

            // Update cooldown text
            _cooldownText.Text = currentCooldown.ToString("F1");
        }
    }

    public void UpdateState(Ability.AbilityState state)
    {
        if (_currentAbility == null) return;

        // Only update if state has changed
        if (_currentState == state) return;

        _currentState = state;

        // Reset previous state visuals
        Modulate = new Color(1, 1, 1);

        // Apply visual state based on current state
        switch (state)
        {
            case Ability.AbilityState.Default:
                // Default state - normal appearance
                break;

            case Ability.AbilityState.Charging:
                // Visual indicator for charging
                Modulate = new Color(1.2f, 1.2f, 0.8f);
                break;

            case Ability.AbilityState.Channeling:
                // Visual indicator for channeling
                Modulate = new Color(0.8f, 1.2f, 1.2f);
                break;

            case Ability.AbilityState.ToggledOn:
                // Visual indicator for toggled on
                Modulate = new Color(1.2f, 1.2f, 1.5f);
                break;

            case Ability.AbilityState.ToggledOff:
                // Visual indicator for toggled off
                break;

            case Ability.AbilityState.Cooldown:
                // Cooldown is handled by UpdateCooldown method
                break;
        }
    }

    public void OnActivated()
    {
        // Visual feedback when ability is activated - only if not on cooldown
        if (_currentState == Ability.AbilityState.Cooldown) return;

        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1.5f, 1.5f, 1.5f), 0.1);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1);
    }
}