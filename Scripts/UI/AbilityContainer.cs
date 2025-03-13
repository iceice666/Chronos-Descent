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

        // Connect to C# events
        _abilityManager.AbilityActivated += OnAbilityActivated;
        _abilityManager.AbilityCooldownChanged += OnAbilityCooldownChanged;
        _abilityManager.AbilityStateChanged += OnAbilityStateChanged;
        _abilityManager.AbilityChanged += OnAbilityChanged;
        UserInputManager.Instance.InputSourceChanged += OnInputSourceChanged;

        // Initialize all slots with current abilities
        UpdateAllSlots();

        GD.Print("Ability container initialized");
    }

    private void CreateAbilitySlots()
    {
        // Create a slot for each ability type
        CreateAbilitySlot(AbilityManagerComponent.Slot.NormalAttack);
        CreateAbilitySlot(AbilityManagerComponent.Slot.Primary);
        CreateAbilitySlot(AbilityManagerComponent.Slot.Secondary);
        CreateAbilitySlot(AbilityManagerComponent.Slot.WeaponUlt);
    }

    private void CreateAbilitySlot(AbilityManagerComponent.Slot slotType)
    {
        var slot = _abilitySlotScene.Instantiate<AbilitySlot>();
        AddChild(slot);

        // Set up the slot
        slot.Name = $"Slot_{slotType}";
        slot.SlotType = slotType;
        slot.UpdateHotKeyLabel();

        // Store reference
        _abilitySlots[(int)slotType] = slot;
    }

    private void UpdateAllSlots()
    {
        if (_abilityManager == null) return;

        foreach (var slot in _abilitySlots)
        {
            if (slot == null) continue;
            
            var ability = _abilityManager.GetAbility(slot.SlotType);
            slot.UpdateAbility(ability);

            if (ability == null) continue;

            // Set initial cooldown state
            slot.UpdateCooldown(ability.CurrentCooldown, ability.Cooldown);

            // Set initial state
            Ability.AbilityState state;
            if (ability.IsCharging) 
                state = Ability.AbilityState.Charging;
            else if (ability.IsChanneling) 
                state = Ability.AbilityState.Channeling;
            else if (ability.IsToggled) 
                state = Ability.AbilityState.ToggledOn;
            else if (ability.IsOnCooldown)
                state = Ability.AbilityState.Cooldown;
            else 
                state = Ability.AbilityState.Default;
                
            slot.UpdateState(state);
        }
    }

    private void OnAbilityActivated(object sender, AbilityManagerComponent.AbilityEventArgs e)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null) continue;
            
            if (_abilityManager.GetAbility(slot.SlotType) != e.Ability) continue;
            slot.OnActivated();
            break;
        }
    }

    private void OnAbilityCooldownChanged(object sender, AbilityManagerComponent.AbilityCooldownEventArgs e)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null) continue;
            
            if (_abilityManager.GetAbility(slot.SlotType) != e.Ability) continue;
            slot.UpdateCooldown(e.Cooldown, e.Ability.Cooldown);
            break;
        }
    }

    private void OnAbilityStateChanged(object sender, AbilityManagerComponent.AbilityStateEventArgs e)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null) continue;
            
            if (_abilityManager.GetAbility(slot.SlotType) != e.Ability) continue;
            slot.UpdateState(e.State);
            break;
        }
    }

    private void OnAbilityChanged(object sender, AbilityManagerComponent.AbilitySlotEventArgs e)
    {
        if ((int)e.SlotValue >= _abilitySlots.Length || _abilitySlots[(int)e.SlotValue] == null) return;
        
        var abilitySlot = _abilitySlots[(int)e.SlotValue];
        abilitySlot.UpdateAbility(e.Ability);
    }

    private void OnInputSourceChanged(UserInputManager.InputSource inputSource)
    {
        foreach (var slot in _abilitySlots)
        {
            slot?.UpdateHotKeyLabel();
        }
    }

    public override void _ExitTree()
    {
        // Disconnect events
        if (_abilityManager == null) return;

        _abilityManager.AbilityActivated -= OnAbilityActivated;
        _abilityManager.AbilityCooldownChanged -= OnAbilityCooldownChanged;
        _abilityManager.AbilityStateChanged -= OnAbilityStateChanged;
        _abilityManager.AbilityChanged -= OnAbilityChanged;
        UserInputManager.Instance.InputSourceChanged -= OnInputSourceChanged;
    }
}