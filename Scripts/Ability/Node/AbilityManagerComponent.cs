using System;
using System.Collections.Generic;
using Godot;

namespace ChronosDescent.Scripts.Ability.Node;

[GlobalClass]
public partial class AbilityManagerComponent : Godot.Node
{
    // List of abilities - sized to match enum values
    private readonly BaseAbility[] _abilities = new BaseAbility[4];

    private readonly Dictionary<
        BaseAbility,
        EventHandler<BaseAbility.AbilityCooldownEventArgs>
    > _cooldownChangedHandlers = new();

    // Dictionary to store references to event handlers for unsubscribing
    private readonly Dictionary<
        BaseAbility,
        EventHandler<BaseAbility.AbilityStateEventArgs>
    > _stateChangedHandlers = new();

    private Entity.Entity _caster;

    // Track currently active ability slot
    private AbilitySlot _currentActiveAbilitySlot = AbilitySlot.Unknown;

    // C# events instead of Godot signals
    public event EventHandler<AbilityEventArgs> AbilityActivated;
    public event EventHandler<AbilityCooldownEventArgs> AbilityCooldownChanged;
    public event EventHandler<AbilityStateEventArgs> AbilityStateChanged;
    public event EventHandler<AbilitySlotEventArgs> AbilityChanged;

    public override void _Ready()
    {
        _caster = GetOwner<Entity.Entity>();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateAbilities(delta);
    }

    private void UpdateAbilities(double delta)
    {
        foreach (var ability in _abilities)
            ability?.Update(delta);
    }

    // Add an ability
    public void SetAbility(AbilitySlot abilitySlot, BaseAbility ability)
    {
        if (abilitySlot == AbilitySlot.Unknown || abilitySlot != ability.RequiredSlot)
            return;

        // If slot already has an ability, clean it up first
        var existingAbility = GetAbility(abilitySlot);
        if (existingAbility != null)
        {
            UnsubscribeFromAbilityEvents(existingAbility);
            existingAbility.Caster = null;
        }

        var caster = (Entity.Entity)Owner;

        ability.Caster = caster;
        ability.Initialize();
        _abilities[(int)abilitySlot] = ability;

        // Subscribe to ability events
        SubscribeToAbilityEvents(ability, abilitySlot);

        OnAbilityChanged(new AbilitySlotEventArgs(ability, abilitySlot));
        GD.Print($"Added ability {ability.Name} to {caster.Name}");
    }

    // Remove an ability
    public void RemoveAbility(AbilitySlot abilitySlot)
    {
        if (abilitySlot == AbilitySlot.Unknown)
            return;

        var index = (int)abilitySlot;
        var ability = _abilities[index];

        if (ability == null)
            return;

        UnsubscribeFromAbilityEvents(ability);
        ability.Caster = null;
        _abilities[index] = null;

        // Clear active slot if removing active ability
        if (_currentActiveAbilitySlot == abilitySlot)
            _currentActiveAbilitySlot = AbilitySlot.Unknown;

        OnAbilityChanged(new AbilitySlotEventArgs(null, abilitySlot));
        GD.Print($"Removed ability {ability.Name} from slot {abilitySlot}");
    }

    // Subscribe to ability events
    private void SubscribeToAbilityEvents(BaseAbility ability, AbilitySlot abilitySlot)
    {
        // Create handler for state changed
        EventHandler<BaseAbility.AbilityStateEventArgs> stateHandler = (sender, e) =>
            HandleAbilityStateChanged(ability, abilitySlot, e.State);

        // Create handler for cooldown changed
        EventHandler<BaseAbility.AbilityCooldownEventArgs> cooldownHandler = (sender, e) =>
            HandleAbilityCooldownChanged(ability);

        // Store handlers for later unsubscription
        _stateChangedHandlers[ability] = stateHandler;
        _cooldownChangedHandlers[ability] = cooldownHandler;

        // Subscribe to events
        ability.StateChanged += stateHandler;
        ability.CooldownChanged += cooldownHandler;
    }

    // Unsubscribe from ability events
    private void UnsubscribeFromAbilityEvents(BaseAbility ability)
    {
        if (_stateChangedHandlers.TryGetValue(ability, out var stateHandler))
        {
            ability.StateChanged -= stateHandler;
            _stateChangedHandlers.Remove(ability);
        }

        if (_cooldownChangedHandlers.TryGetValue(ability, out var cooldownHandler))
        {
            ability.CooldownChanged -= cooldownHandler;
            _cooldownChangedHandlers.Remove(ability);
        }
    }

    // Handle ability state changed
    private void HandleAbilityStateChanged(BaseAbility ability, AbilitySlot abilitySlot, BaseAbility.AbilityState state)
    {
        if (abilitySlot == AbilitySlot.Unknown || ability == null) return;

        switch (state)
        {
            case BaseAbility.AbilityState.Channeling or BaseAbility.AbilityState.Charging:
                _currentActiveAbilitySlot = abilitySlot;
                break;
            case BaseAbility.AbilityState.Cooldown:
                _caster.Moveable = true;
                break;
            case BaseAbility.AbilityState.Default when GetSlotForAbility(ability) == _currentActiveAbilitySlot:
                _currentActiveAbilitySlot = AbilitySlot.Unknown;
                break;
        }


        OnAbilityStateChanged(new AbilityStateEventArgs(ability, state));
    }

    // Redirector
    private void HandleAbilityCooldownChanged(BaseAbility ability)
    {
        OnAbilityCooldownChanged(new AbilityCooldownEventArgs(ability, ability.CurrentCooldown));
    }

    // Get an ability by slot
    public BaseAbility GetAbility(AbilitySlot abilitySlot)
    {
        if (abilitySlot == AbilitySlot.Unknown || (int)abilitySlot >= _abilities.Length)
            return null;
        return _abilities[(int)abilitySlot];
    }

    // Get the slot for a specific ability
    private AbilitySlot GetSlotForAbility(BaseAbility ability)
    {
        for (var i = 0; i < _abilities.Length; i++)
            if (_abilities[i] == ability)
                return (AbilitySlot)i;

        return AbilitySlot.Unknown;
    }

    // Activate an ability by slot
    public void ActivateAbility(AbilitySlot abilitySlot)
    {
        if (abilitySlot == AbilitySlot.Unknown)
            return;

        var ability = GetAbility(abilitySlot);
        switch (ability)
        {
            case null:
                GD.Print($"Ability slot {abilitySlot} is empty");
                return;

            // Don't allow activating a new channeled/charged ability while another is active
            case BaseChanneledAbility
                or BaseChargedAbility
                when _currentActiveAbilitySlot != AbilitySlot.Unknown
                     && _currentActiveAbilitySlot != abilitySlot:
            {
                if (!IsAbilityOnCooldown(_currentActiveAbilitySlot))
                {
                    GD.Print($"Cannot activate {ability.Name} while another ability is active");
                    return;
                }

                _currentActiveAbilitySlot = AbilitySlot.Unknown;
                break;
            }
        }

        if (!ability.CanActivate())
        {
            GD.Print($"Cannot activate {ability.Name}");
            return;
        }

        ability.Activate();
        OnAbilityActivated(new AbilityEventArgs(ability));

        _caster.Moveable = false;

        GD.Print($"Activated ability {ability.Name}");
    }

    // Release a charged ability
    public void ReleaseChargedAbility(AbilitySlot abilitySlot)
    {
        if (abilitySlot != _currentActiveAbilitySlot && abilitySlot != AbilitySlot.Unknown)
            // Only process if the requested slot is the active one
            return;

        if (_currentActiveAbilitySlot == AbilitySlot.Unknown)
        {
            Util.PrintWarning("Attempting to release a unknown charged ability");
            return;
        }

        var ability = GetAbility(_currentActiveAbilitySlot);
        if (ability is not BaseChargedAbility ab)
            return;
        if (ab.IsCharging != true)
            return;

        ab.ReleaseCharge();
        _currentActiveAbilitySlot = AbilitySlot.Unknown;

        ((Entity.Entity)Owner).Moveable = true;

        GD.Print($"Released charge of {ability.Name}");
    }

    // Cancel a charging ability
    public void CancelChargedAbility()
    {
        if (_currentActiveAbilitySlot != AbilitySlot.Unknown)
        {
            var ability = GetAbility(_currentActiveAbilitySlot);
            if (ability is not BaseChargedAbility ab)
                return;
            if (ab.IsCharging)
            {
                ab.CancelCharge();
                _currentActiveAbilitySlot = AbilitySlot.Unknown;
            }
            else
            {
                Util.PrintWarning("Current ability is not charging");
            }
        }
        else
        {
            Util.PrintWarning("No ability is charging");
        }
    }

    // Interrupt a channeling ability
    public void InterruptChannelingAbility()
    {
        if (_currentActiveAbilitySlot == AbilitySlot.Unknown)
        {
            Util.PrintWarning("Attempting to interrupt a unknown channeling ability");
            return;
        }

        var ability = GetAbility(_currentActiveAbilitySlot);

        if (ability is not BaseChanneledAbility ab)
            return;
        if (ab.IsChanneling != true)
            return;

        ab.InterruptChanneling();
        _currentActiveAbilitySlot = AbilitySlot.Unknown;

        GD.Print($"Interrupted channeling of {ability.Name}");
    }

    public static AbilitySlot[] GetAllSlots()
    {
        return
        [
            AbilitySlot.WeaponAttack,
            AbilitySlot.WeaponUlt,
            AbilitySlot.WeaponSpecial,
            AbilitySlot.LifeSaving
        ];
    }

    // Custom event args
    public class AbilityEventArgs : EventArgs
    {
        public AbilityEventArgs(BaseAbility ability)
        {
            Ability = ability;
        }

        public BaseAbility Ability { get; }
    }

    public class AbilityCooldownEventArgs : EventArgs
    {
        public AbilityCooldownEventArgs(BaseAbility ability, double cooldown)
        {
            Ability = ability;
            Cooldown = cooldown;
        }

        public BaseAbility Ability { get; }
        public double Cooldown { get; }
    }

    public class AbilityStateEventArgs : EventArgs
    {
        public AbilityStateEventArgs(BaseAbility ability, BaseAbility.AbilityState state)
        {
            Ability = ability;
            State = state;
        }

        public BaseAbility Ability { get; }
        public BaseAbility.AbilityState State { get; }
    }

    public class AbilitySlotEventArgs : EventArgs
    {
        public AbilitySlotEventArgs(BaseAbility ability, AbilitySlot abilitySlot)
        {
            Ability = ability;
            AbilitySlotValue = abilitySlot;
        }

        public BaseAbility Ability { get; }
        public AbilitySlot AbilitySlotValue { get; }
    }

    #region Event Invokers

    protected void OnAbilityActivated(AbilityEventArgs e)
    {
        AbilityActivated?.Invoke(this, e);
    }

    protected void OnAbilityCooldownChanged(AbilityCooldownEventArgs e)
    {
        AbilityCooldownChanged?.Invoke(this, e);
    }

    protected void OnAbilityStateChanged(AbilityStateEventArgs e)
    {
        AbilityStateChanged?.Invoke(this, e);
    }

    protected void OnAbilityChanged(AbilitySlotEventArgs e)
    {
        AbilityChanged?.Invoke(this, e);
    }

    #endregion

    #region BaseAbility State Helpers

    // Get the cooldown of an ability
    public double GetAbilityCooldown(AbilitySlot abilitySlot)
    {
        return GetAbility(abilitySlot)?.CurrentCooldown ?? 0.0;
    }

    // Get the cooldown percentage of an ability
    public double GetAbilityCooldownPercentage(AbilitySlot abilitySlot)
    {
        var ability = GetAbility(abilitySlot);
        if (ability == null || ability.Cooldown <= 0.0)
            return 0.0;

        return ability.CurrentCooldown / ability.Cooldown;
    }

    public bool IsAbilityReady(AbilitySlot abilitySlot)
    {
        return GetAbility(abilitySlot)?.CanActivate() ?? false;
    }

    public bool IsAbilityCharging(AbilitySlot abilitySlot)
    {
        var ability = GetAbility(abilitySlot);
        if (ability is BaseChargedAbility ab)
            return ab.IsCharging;

        return false;
    }

    public bool IsAbilityChanneling(AbilitySlot abilitySlot)
    {
        var ability = GetAbility(abilitySlot);
        if (ability is BaseChanneledAbility ab)
            return ab.IsChanneling;

        return false;
    }

    public bool IsAbilityOnCooldown(AbilitySlot abilitySlot)
    {
        return GetAbility(abilitySlot)?.IsOnCooldown ?? false;
    }

    public bool IsAbilityToggled(AbilitySlot abilitySlot)
    {
        var ability = GetAbility(abilitySlot);
        if (ability is BaseToggleAbility ab)
            return ab.IsToggled;

        return false;
    }

    public AbilitySlot GetCurrentActiveSlot()
    {
        return _currentActiveAbilitySlot;
    }

    public bool HasActiveAbility()
    {
        return _currentActiveAbilitySlot != AbilitySlot.Unknown;
    }

    #endregion
}

public enum AbilitySlot
{
    Unknown = -1,
    WeaponAttack = 0,
    WeaponUlt = 1,
    WeaponSpecial = 2,
    LifeSaving = 3
}

public static class AbilitySlotExtensions
{
    public static string GetSlotName(this AbilitySlot abilitySlot)
    {
        switch (abilitySlot)
        {
            case AbilitySlot.WeaponAttack:
                return "weapon_attack";
            case AbilitySlot.WeaponUlt:
                return "weapon_ult";
            case AbilitySlot.WeaponSpecial:
                return "weapon_special";
            case AbilitySlot.LifeSaving:
                return "life_saving";

            case AbilitySlot.Unknown:
            default:
                return "unknown";
        }
    }
}