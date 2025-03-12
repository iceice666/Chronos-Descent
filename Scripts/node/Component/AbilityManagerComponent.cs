using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class AbilityManagerComponent : Node
{
    // Signals
    [Signal]
    public delegate void AbilityActivatedEventHandler(Ability ability);

    [Signal]
    public delegate void AbilityCooldownChangedEventHandler(Ability ability, double cooldown);

    [Signal]
    public delegate void AbilityStateChangedEventHandler(Ability ability, int state);

    public enum Slot
    {
        NormalAttack = 0,
        Primary = 1,
        Secondary = 2,
        WeaponUlt = 3,
        Unknown = 8,
    }

    public enum AbilityState
    {
        Default = 0,
        Charging = 1,
        Channeling = 2,
        ToggledOn = 3,
        ToggledOff = 4,
        Idle = 5
    }

    // List of abilities - sized to match enum values
    private readonly Ability[] _abilities = new Ability[4];

    // Reference to the entity this component is attached to
    private Entity _caster;

    // Track currently active ability slot
    private Slot _currentActiveSlot = Slot.Unknown;

    public override void _Ready()
    {
        _caster = GetParent<Entity>();
    }

    public override void _Process(double delta)
    {
        UpdateAbilities(delta);
    }

    private void UpdateAbilities(double delta)
    {
        for (var i = 0; i < 4; i++)
        {
            var ability = _abilities[i];
            if (ability == null) continue;

            var oldCooldown = ability.CurrentCooldown;
            var wasCharging = ability.IsCharging;
            var wasChanneling = ability.IsChanneling;
            var wasToggled = ability.IsToggled;

            ability.Update(delta);

            // Emit signals for state changes
            if (!Util.NearlyEqual(oldCooldown, ability.CurrentCooldown))
            {
                EmitSignal(SignalName.AbilityCooldownChanged, ability, ability.CurrentCooldown);
            }

            UpdateAbilityStateSignals(ability, wasCharging, wasChanneling, wasToggled);
        }
    }

    private void UpdateAbilityStateSignals(Ability ability, bool wasCharging, bool wasChanneling, bool wasToggled)
    {
        if (wasCharging != ability.IsCharging)
        {
            var newState = ability.IsCharging ? AbilityState.Charging : AbilityState.Idle;
            EmitSignal(SignalName.AbilityStateChanged, ability, (int)newState);
        }

        else if (wasChanneling != ability.IsChanneling)
        {
            var newState = ability.IsChanneling ? AbilityState.Channeling : AbilityState.Idle;
            EmitSignal(SignalName.AbilityStateChanged, ability, (int)newState);
        }

        else if (wasToggled != ability.IsToggled)
        {
            var newState = ability.IsToggled ? AbilityState.ToggledOn : AbilityState.ToggledOff;
            EmitSignal(SignalName.AbilityStateChanged, ability, (int)newState);
        }
    }

    // Add an ability
    public void SetAbility(Slot slot, Ability ability)
    {
        if (slot == Slot.Unknown) return;

        _abilities.SetValue(ability, (int)slot);


        ability.Caster = _caster;
        GD.Print($"Added ability {ability.Name} to {_caster.Name}");
    }

    // Remove an ability
    public void RemoveAbility(Slot slot)
    {
        if (slot == Slot.Unknown) return;

        var index = (int)slot;

        var ability = (Ability)_abilities.GetValue(index);
        if (ability == null) return;

        ability.Caster = null;
        _abilities.SetValue(null, index);
        GD.Print($"Removed ability {ability.Name} from slot {slot}");
    }

    // Get an ability by slot
    public Ability GetAbility(Slot slot)
    {
        if (slot == Slot.Unknown) return null;

        return (Ability)_abilities.GetValue((int)slot);
    }

    // Activate an ability by slot
    public void ActivateAbility(Slot slot)
    {
        if (slot == Slot.Unknown) return;

        var ability = GetAbility(slot);
        if (ability == null)
        {
            GD.Print($"Ability slot {slot} is empty");
            return;
        }

        // Don't allow activating a new channeled/charged ability while another is active
        if (ability.Type is Ability.AbilityType.Channeled or Ability.AbilityType.Charged &&
            _currentActiveSlot != Slot.Unknown && _currentActiveSlot != slot)
        {
            GD.Print($"Cannot activate {ability.Name} while another ability is active");
            return;
        }

        if (!ability.CanActivate())
        {
            GD.Print($"Cannot activate {ability.Name} (not ready)");
            return;
        }

        ability.Activate();

        // Track currently active slot for channeled/charged abilities
        if (ability.Type is Ability.AbilityType.Channeled or Ability.AbilityType.Charged)
        {
            _currentActiveSlot = slot;
        }

        EmitSignal(SignalName.AbilityActivated, ability);
        GD.Print($"Activated ability {ability.Name}");
    }

    // Release a charged ability
    public void ReleaseChargedAbility()
    {
        if (_currentActiveSlot != Slot.Unknown)
        {
            var ability = GetAbility(_currentActiveSlot);
            if (ability?.IsCharging != true) return;

            ability.ReleaseCharge();

            _currentActiveSlot = Slot.Unknown;


            GD.Print($"Released charge of {ability.Name}");
        }
        else
        {
            GD.PushWarning($"Attempting to release a unknown charged ability: {_currentActiveSlot}");
        }
    }


    // Cancel a charging ability
    public void CancelChargedAbility()
    {
        if (_currentActiveSlot != Slot.Unknown)
        {
            var ability = GetAbility(_currentActiveSlot);
            if (ability?.IsCharging != true) return;

            ability.CancelCharge();

            _currentActiveSlot = Slot.Unknown;
            GD.Print($"Canceled charge of {ability.Name}");
        }
        else
        {
            GD.PushWarning($"Attempting to cancel a unknown charged ability: {_currentActiveSlot}");
        }
    }


    // Interrupt a channeling ability
    public void InterruptChannelingAbility()
    {
        if (_currentActiveSlot != Slot.Unknown)
        {
            var ability = GetAbility(_currentActiveSlot);
            if (ability?.IsChanneling != true) return;
            ability.InterruptChanneling();


            _currentActiveSlot = Slot.Unknown;


            GD.Print($"Interrupted channeling of {ability.Name}");
        }
        else
        {
            GD.PushWarning($"Attempting to interrupt a unknown charged ability: {_currentActiveSlot}");
        }
    }

    #region Ability State Helpers

    // Get the cooldown of an ability
    public double GetAbilityCooldown(Slot slot)
        => GetAbility(slot)?.CurrentCooldown ?? 0.0;


    // Get the cooldown percentage of an ability
    public double GetAbilityCooldownPercentage(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability == null || ability.Cooldown <= 0.0) return 0.0;

        return ability.CurrentCooldown / ability.Cooldown;
    }


    public bool IsAbilityReady(Slot slot)
        => GetAbility(slot)?.CanActivate() ?? false;

    public bool IsAbilityCharging(Slot slot)
        => GetAbility(slot)?.IsCharging ?? false;


    public bool IsAbilityChanneling(Slot slot)
        => GetAbility(slot)?.IsChanneling ?? false;


    public bool IsAbilityOnCooldown(Slot slot)
        => GetAbility(slot)?.IsOnCooldown ?? false;


    public bool IsAbilityToggled(Slot slot)
        => GetAbility(slot)?.IsToggled ?? false;


    public Slot GetCurrentActiveSlot()
        => _currentActiveSlot;


    public bool HasActiveAbility()
        => _currentActiveSlot != Slot.Unknown;

    #endregion
}