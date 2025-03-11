using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AbilityContainer : HBoxContainer
{
    // Reference to the player's ability manager
    private AbilityManagerComponent _abilityManager;

    // Dictionary to store ability UI slots
    private readonly AbilitySlot[] _abilitySlots = new AbilitySlot[4];

    // Reference to the ability slot scene
    private PackedScene _abilitySlotScene;

    public override void _Ready()
    {
        // Load the ability slot scene
        _abilitySlotScene = GD.Load<PackedScene>("res://Scenes/ui/ability_slot.tscn");

        // Set theme constants for proper spacing
        AddThemeConstantOverride("separation", 8);

        // Create slots for all abilities
        CreateAbilitySlots();
    }

    public void Initialize(Entity entity)
    {
        if (entity == null) return;

        _abilityManager = entity.AbilityManager;

        // Connect to signals
        _abilityManager.AbilityActivated += OnAbilityActivated;
        _abilityManager.AbilityCooldownChanged += OnAbilityCooldownChanged;
        _abilityManager.AbilityStateChanged += OnAbilityStateChanged;
        UserInputManager.Instance.InputSourceChanged += OnInputSourceChanged;

        // Initialize all slots with current abilities
        UpdateAllSlots();

        GD.Print("Ability container initialized");
    }

    private void CreateAbilitySlots()
    {
        // Create a slot for each ability type
        foreach (AbilityManagerComponent.Slot slotType in System.Enum.GetValues(typeof(AbilityManagerComponent.Slot)))
        {
            var slot = _abilitySlotScene.Instantiate<AbilitySlot>();
            AddChild(slot);

            // Set up the slot
            slot.Name = $"Slot_{slotType}";
            slot.SlotType = slotType;
            slot.UpdateHotKeyLabel();


            // Store reference
            _abilitySlots.SetValue(slot, (int)slotType);
        }
    }

    private void UpdateAllSlots()
    {
        if (_abilityManager == null) return;

        foreach (var slot in _abilitySlots)
        {
            var ability = _abilityManager.GetAbility(slot.SlotType);

            slot.UpdateAbility(ability);

            if (ability == null) continue;

            // Set initial cooldown state
            slot.UpdateCooldown(ability.CurrentCooldown, ability.Cooldown);

            // Set initial state
            if (ability.IsCharging) slot.UpdateState(AbilityManagerComponent.AbilityState.Charging);
            else if (ability.IsChanneling) slot.UpdateState(AbilityManagerComponent.AbilityState.Channeling);
            else if (ability.IsToggled) slot.UpdateState(AbilityManagerComponent.AbilityState.ToggledOn);
            else slot.UpdateState(AbilityManagerComponent.AbilityState.Idle);
        }
    }

    private void OnAbilityActivated(Ability ability)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (_abilityManager.GetAbility(slot.SlotType) != ability) continue;
            slot.OnActivated();
            break;
        }
    }

    private void OnAbilityCooldownChanged(Ability ability, double cooldown)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (_abilityManager.GetAbility(slot.SlotType) != ability) continue;
            slot.UpdateCooldown(cooldown, ability.Cooldown);
            break;
        }
    }

    private void OnAbilityStateChanged(Ability ability, AbilityManagerComponent.AbilityState state)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (_abilityManager.GetAbility(slot.SlotType) != ability) continue;
            slot.UpdateState(state);
            break;
        }
    }

    private void OnInputSourceChanged(UserInputManager.InputSource _)
    {
        foreach (var slot in _abilitySlots)
        {
            slot?.UpdateHotKeyLabel();
        }
    }

    public override void _ExitTree()
    {
        // Disconnect signals
        if (_abilityManager == null) return;

        _abilityManager.AbilityActivated -= OnAbilityActivated;
        _abilityManager.AbilityCooldownChanged -= OnAbilityCooldownChanged;
        _abilityManager.AbilityStateChanged -= OnAbilityStateChanged;
        UserInputManager.Instance.InputSourceChanged -= OnInputSourceChanged;
    }
}