using ChronosDescent.Scripts.Ability.Node;
using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.Ability.UI;

public partial class AbilityContainer : HBoxContainer
{
    // Dictionary to store ability UI slots
    private readonly AbilityUiSlot[] _abilitySlots = new AbilityUiSlot[4];

    // Reference to the player's ability manager
    private AbilityManagerComponent _abilityManager;

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

    public void Initialize(Entity.Entity entity)
    {
        if (entity == null)
            return;

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
        foreach (var slot in AbilityManagerComponent.GetAllSlots())
            CreateAbilitySlot(slot);
    }

    private void CreateAbilitySlot(AbilitySlot slotType)
    {
        var slot = _abilitySlotScene.Instantiate<AbilityUiSlot>();
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
        if (_abilityManager == null)
            return;

        foreach (var slot in _abilitySlots)
        {
            if (slot == null)
                continue;

            var ability = _abilityManager.GetAbility(slot.SlotType);
            slot.UpdateAbility(ability);

            if (ability == null)
                continue;

            // Set initial cooldown state
            slot.UpdateCooldown(ability.CurrentCooldown, ability.Cooldown);

            // Set initial state
            BaseAbility.AbilityState state;
            if (ability is BaseChargedAbility { IsCharging: true })
                state = BaseAbility.AbilityState.Charging;
            else if (ability is BaseChanneledAbility { IsChanneling: true })
                state = BaseAbility.AbilityState.Channeling;
            else if (ability is BaseToggleAbility { IsToggled: true })
                state = BaseAbility.AbilityState.ToggledOn;
            else if (ability.IsOnCooldown)
                state = BaseAbility.AbilityState.Cooldown;
            else
                state = BaseAbility.AbilityState.Default;

            slot.UpdateState(state);
        }
    }

    private void OnAbilityActivated(BaseAbility ability)
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null)
                continue;

            if (_abilityManager.GetAbility(slot.SlotType) != ability)
                continue;
            slot.OnActivated();
            break;
        }
    }

    private void OnAbilityCooldownChanged(BaseAbility ability, double cooldown
    )
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null)
                continue;

            if (_abilityManager.GetAbility(slot.SlotType) != ability)
                continue;
            slot.UpdateCooldown(cooldown, ability.Cooldown);
            break;
        }
    }

    private void OnAbilityStateChanged(BaseAbility ability, BaseAbility.AbilityState state
    )
    {
        // Find the slot for this ability
        foreach (var slot in _abilitySlots)
        {
            if (slot == null)
                continue;

            if (_abilityManager.GetAbility(slot.SlotType) != ability)
                continue;
            slot.UpdateState(state);
            break;
        }
    }

    private void OnAbilityChanged(BaseAbility ability, AbilitySlot abilitySlot)
    {
        if ((int)abilitySlot >= _abilitySlots.Length) return;

        _abilitySlots[(int)abilitySlot]?.UpdateAbility(ability);
    }

    private void OnInputSourceChanged(UserInputManager.InputSource inputSource)
    {
        foreach (var slot in _abilitySlots)
            slot?.UpdateHotKeyLabel();
    }

    public override void _ExitTree()
    {
        // Disconnect events
        if (_abilityManager == null)
            return;

        _abilityManager.AbilityActivated -= OnAbilityActivated;
        _abilityManager.AbilityCooldownChanged -= OnAbilityCooldownChanged;
        _abilityManager.AbilityStateChanged -= OnAbilityStateChanged;
        _abilityManager.AbilityChanged -= OnAbilityChanged;
        UserInputManager.Instance.InputSourceChanged -= OnInputSourceChanged;
    }
}