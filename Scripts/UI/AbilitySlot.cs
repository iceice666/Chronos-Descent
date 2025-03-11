using System;
using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AbilitySlot : Panel
{
    // UI components
    private TextureRect _iconNode;
    private Label _hotKeyLabel;
    private Label _nameLabel;
    private ColorRect _cooldownOverlay;
    private TextureProgressBar _chargeBar;

    // Slot properties
    public AbilityManagerComponent.Slot SlotType { get; set; }
    private Ability _currentAbility;

    public override void _Ready()
    {
        // Get references to UI components
        _iconNode = GetNode<TextureRect>("Icon");
        _hotKeyLabel = GetNode<Label>("HotKey");
        _nameLabel = GetNode<Label>("Name");
        _cooldownOverlay = GetNode<ColorRect>("CooldownOverlay");
        _chargeBar = GetNode<TextureProgressBar>("ChargeBar");

        // Initialize with empty state
        _cooldownOverlay.Visible = false;
        _chargeBar.Visible = false;
        _chargeBar.Value = 0;
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
            _chargeBar.Visible = false;
            return;
        }

        // Update with ability info
        _iconNode.Texture = ability.Icon;
        _nameLabel.Text = ability.Name;
    }

    public void UpdateCooldown(double currentCooldown, double maxCooldown)
    {
        if (_currentAbility == null) return;

        if (currentCooldown <= 0)
        {
            // Ability is ready
            _cooldownOverlay.Visible = false;
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
        }
    }


    public void UpdateState(AbilityManagerComponent.AbilityState state)
    {
        if (_currentAbility == null) return;

        switch (state)
        {
            case AbilityManagerComponent.AbilityState.Charging:
                // Show and update charge bar
                _chargeBar.Visible = true;
                _chargeBar.Value = (float)(_currentAbility.CurrentChargeTime / _currentAbility.MaxChargeTime * 100);
                break;

            case AbilityManagerComponent.AbilityState.Channeling:
                // Show and update channeling indicator
                _chargeBar.Visible = true;
                _chargeBar.Value =
                    (float)(_currentAbility.CurrentChannelingTime / _currentAbility.ChannelingDuration * 100);
                break;

            case AbilityManagerComponent.AbilityState.ToggledOn:
                // Show toggled state
                Modulate = new Color(1.2f, 1.2f, 1.5f);
                break;

            case AbilityManagerComponent.AbilityState.ToggledOff:
            case AbilityManagerComponent.AbilityState.Idle:
            case AbilityManagerComponent.AbilityState.Default:
                // Reset to normal state
                _chargeBar.Visible = false;
                Modulate = new Color(1, 1, 1);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void OnActivated()
    {
        // Visual feedback when ability is activated
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1.5f, 1.5f, 1.5f), 0.1);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1);
    }
}