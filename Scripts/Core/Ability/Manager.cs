using System;
using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Core.Ability;

public class Manager : ISystem
{
    private readonly AbilitySlots _abilitySlots = new();
    private BaseEntity _owner;
    public AbilitySlotType CurrentActiveAbilitySlot { get; private set; } = AbilitySlotType.Unknown;

    # region Life Cycle

    public void Initialize(BaseEntity owner)
    {
        _owner = owner;
        _owner.EventBus.Subscribe<BaseAbility>(EventVariant.AbilityStateChange, ability =>
        {
            if (ability.State == AbilityState.Cooldown && ability == GetAbility(CurrentActiveAbilitySlot))
            {
                CurrentActiveAbilitySlot = AbilitySlotType.Unknown;
                _owner.Moveable = true;
            }
        });
    }

    public void Update(double deltaTime)
    {
    }

    public void FixedUpdate(double deltaTime)
    {
        _abilitySlots.Update(deltaTime);
    }

    #endregion

    #region Change Ability

    public void SetAbility(AbilitySlotType slot, BaseAbility ability)
    {
        if (ability == null || slot == AbilitySlotType.Unknown) return;

        // If slot already has an ability, clean it up first
        var existingAbility = GetAbility(slot);
        if (existingAbility != null) existingAbility.Caster = null;

        ability.Initialize(_owner);
        _abilitySlots.SetAbility(ability, slot);

        _owner.EventBus.Publish(
            EventVariant.AbilityChange,
            slot
        );
    }

    public void RemoveAbility(AbilitySlotType slot)
    {
        if (slot == AbilitySlotType.Unknown || CurrentActiveAbilitySlot == slot) return;

        var ability = GetAbility(slot);
        if (ability == null) return;

        ability.Caster = null;

        _abilitySlots.SetAbility(null, slot);

        _owner.EventBus.Publish(
            EventVariant.AbilityChange,
            slot
        );
    }

    #endregion

    #region Interative

    public void ActivateAbility(AbilitySlotType slot)
    {
        var ability = GetAbility(slot);
        if (ability == null
            || (CurrentActiveAbilitySlot != AbilitySlotType.Unknown &&
                ability is BaseChanneledAbility or BaseChargedAbility)
            || !ability.CanActivate()
           ) return;


        _owner.Moveable = false;
        ability.Activate();

        if (ability is BaseChanneledAbility or BaseChargedAbility) CurrentActiveAbilitySlot = slot;

        _owner.EventBus.Publish(
            EventVariant.AbilityActivate,
            ability
        );
    }

    public void ReleaseAbility(AbilitySlotType slot)
    {
        if ((slot != AbilitySlotType.Unknown && CurrentActiveAbilitySlot != slot) ||
            CurrentActiveAbilitySlot == AbilitySlotType.Unknown) return;


        var ability = GetAbility(CurrentActiveAbilitySlot);

        // Currently only Charged Ability have ability to "release".
        if (ability is not BaseChargedAbility ab) return;

        if (ab.CurrentChargeTime < ab.MinChargeTime)
            ab.CancelCharge();
        else
            ab.ReleaseCharge();

        CurrentActiveAbilitySlot = AbilitySlotType.Unknown;
    }

    public void CancelAbility(AbilitySlotType slot)
    {
        if (CurrentActiveAbilitySlot == AbilitySlotType.Unknown) return;
        var ability = GetAbility(CurrentActiveAbilitySlot);
        // Currently only Charged Ability have ability to "cancel".
        if (ability is not BaseChargedAbility ab) return;
        ab.CancelCharge();

        CurrentActiveAbilitySlot = AbilitySlotType.Unknown;
    }

    public void InterruptAbility(AbilitySlotType slot)
    {
        if (CurrentActiveAbilitySlot == AbilitySlotType.Unknown) return;
        var ability = GetAbility(CurrentActiveAbilitySlot);

        switch (ability)
        {
            case BaseChanneledAbility channeledAbility:
                channeledAbility.InterruptChanneling();
                break;
            case BaseChargedAbility chargedAbility:
                chargedAbility.CancelCharge();
                break;
            default:
                return;
        }

        CurrentActiveAbilitySlot = AbilitySlotType.Unknown;
    }

    #endregion

    #region Helpers

    public BaseAbility GetAbility(AbilitySlotType slot)
    {
        return _abilitySlots.GetAbility(slot);
    }

    public bool CanActivateAbility(AbilitySlotType slot)
    {
        return GetAbility(slot)?.CanActivate() ?? false;
    }

    /// <summary>
    /// Get all active abilities currently equipped
    /// </summary>
    public List<BaseAbility> GetAllAbilities()
    {
        var abilities = new List<BaseAbility>();
        
        // Check each slot for an ability
        foreach (AbilitySlotType slot in Enum.GetValues(typeof(AbilitySlotType)))
        {
            if (slot == AbilitySlotType.Unknown) continue;
            
            var ability = GetAbility(slot);
            if (ability != null)
            {
                abilities.Add(ability);
            }
        }
        
        return abilities;
    }

    #endregion
}

public class AbilitySlots
{
    private BaseAbility _lifeSaveAbility;
    private BaseAbility _normalAbility;
    private BaseAbility _specialAbility;
    private BaseAbility _ultimateAbility;

    public void Update(double deltaTime)
    {
        _lifeSaveAbility?.Update(deltaTime);
        _normalAbility?.Update(deltaTime);
        _specialAbility?.Update(deltaTime);
        _ultimateAbility?.Update(deltaTime);
    }

    public void SetAbility(BaseAbility ability, AbilitySlotType slot)
    {
        switch (slot)
        {
            case AbilitySlotType.Normal:
                _normalAbility = ability;
                break;
            case AbilitySlotType.Special:
                _specialAbility = ability;
                break;
            case AbilitySlotType.Ultimate:
                _ultimateAbility = ability;
                break;
            case AbilitySlotType.LifeSaving:
                _lifeSaveAbility = ability;
                break;

            case AbilitySlotType.Unknown:
            default:
                return;
        }
    }

    public BaseAbility GetAbility(AbilitySlotType slot)
    {
        return slot switch
        {
            AbilitySlotType.Unknown => null,
            AbilitySlotType.Normal => _normalAbility,
            AbilitySlotType.Special => _specialAbility,
            AbilitySlotType.Ultimate => _ultimateAbility,
            AbilitySlotType.LifeSaving => _lifeSaveAbility,
            _ => null
        };
    }
}

public enum AbilitySlotType
{
    Unknown = -1,
    Normal = 0,
    Special = 1,
    Ultimate = 2,
    LifeSaving = 3
}

public static class AbilitySlotExtensions
{
    public static string GetSlotName(this AbilitySlotType abilitySlot)
    {
        switch (abilitySlot)
        {
            case AbilitySlotType.Normal:
                return "weapon_normal";
            case AbilitySlotType.Ultimate:
                return "weapon_ult";
            case AbilitySlotType.Special:
                return "weapon_special";
            case AbilitySlotType.LifeSaving:
                return "life_saving";

            case AbilitySlotType.Unknown:
            default:
                return "unknown";
        }
    }
}