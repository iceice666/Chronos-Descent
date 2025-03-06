using System.Collections.Generic;
using Godot;
using ChronosDescent.Scripts.resource;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class AbilityManagerComponent : Node
{
    public enum Slot
    {
        Primary,
        Secondary,
        Tertiary,
        WeaponUlt
    }

    // List of abilities
    private readonly Ability[] _abilities = [];

    // Reference to the entity this component is attached to
    private Entity _caster;

    // Signals
    [Signal]
    public delegate void AbilityActivatedEventHandler(Ability ability);

    [Signal]
    public delegate void AbilityCooldownChangedEventHandler(Ability ability, double cooldown);

    [Signal]
    public delegate void AbilityStateChangedEventHandler(Ability ability, string state);

    public override void _Ready()
    {
        _caster = GetParent<Entity>();
    }

    public override void _Process(double delta)
    {
        // Update all abilities
        foreach (var ability in _abilities)
        {
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

            if (wasCharging != ability.IsCharging)
            {
                EmitSignal(SignalName.AbilityStateChanged, ability, ability.IsCharging ? "charging" : "idle");
            }

            if (wasChanneling != ability.IsChanneling)
            {
                EmitSignal(SignalName.AbilityStateChanged, ability, ability.IsChanneling ? "channeling" : "idle");
            }

            if (wasToggled != ability.IsToggled)
            {
                EmitSignal(SignalName.AbilityStateChanged, ability, ability.IsToggled ? "toggled_on" : "toggled_off");
            }
        }
    }

    // Add an ability
    public void SetAbility(Slot slot, Ability ability)
    {
        _abilities[(int)slot] = ability;
        ability.Caster = _caster;
        GD.Print($"Added ability {ability.Name} to {_caster.Name}");
    }

    // Remove an ability
    public void RemoveAbility(Slot slot)
    {
        _abilities[(int)slot].Caster = null;
        _abilities[(int)slot] = null;
        GD.Print($"Removed ability {slot}");
    }

    // Get an ability by name
    public Ability GetAbility(Slot slot) => _abilities[(int)slot];

    // Get all abilities
    public IEnumerable<Ability> GetAbilities() => _abilities;


    // Activate an ability by name
    public void ActivateAbility(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability != null)
        {
            ability.Activate();
            EmitSignal(SignalName.AbilityActivated, ability);
        }
        else
        {
            GD.Print($"Ability slot {slot} is empty");
        }
    }


    // Release a charged ability
    public void ReleaseChargedAbility(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability is { IsCharging: true })
        {
            ability.ReleaseCharge();
        }
    }

    // Cancel a charging ability
    public void CancelChargedAbility(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability is { IsCharging: true })
        {
            ability.CancelCharge();
        }
    }

    // Interrupt a channeling ability
    public void InterruptChannelingAbility(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability is { IsChanneling: true })
        {
            ability.InterruptChanneling();
        }
    }

    // Toggle an ability
    public void ToggleAbility(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability is { Type: Ability.AbilityType.Toggle })
        {
            ability.Activate(); // For toggle abilities, Activate toggles them
        }
    }

    // Get the cooldown of an ability
    public double GetAbilityCooldown(Slot slot) => GetAbility(slot)?.CurrentCooldown ?? 0.0;

    // Get the cooldown percentage of an ability
    public double GetAbilityCooldownPercentage(Slot slot)
    {
        var ability = GetAbility(slot);
        if (ability == null || ability.Cooldown <= 0.0) return 0.0;

        return ability.CurrentCooldown / ability.Cooldown;
    }

    public bool IsAbilityReady(Slot slot) => GetAbility(slot)?.CanActivate() ?? false;
    public bool IsAbilityCharging(Slot slot) => GetAbility(slot)?.IsCharging ?? false;
    public bool IsAbilityChanneling(Slot slot) => GetAbility(slot)?.IsChanneling ?? false;
    public bool IsAbilityOnCooldown(Slot slot) => GetAbility(slot)?.IsOnCooldown ?? false;
    public bool IsAbilityToggled(Slot slot) => GetAbility(slot)?.IsToggled ?? false;
}