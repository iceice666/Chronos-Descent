using System;
using ChronosDescent.Scripts.node;
using Godot;

namespace ChronosDescent.Scripts.resource;

[GlobalClass]
public partial class Ability : Resource
{
    // Ability type
    public enum AbilityType
    {
        Active, // Standard one-time use ability
        Passive, // Always active ability
        Toggle, // Can be turned on/off
        Charged, // Hold to charge up, release to activate
        Channeled // Continuous effect while channeling
    }

    public Entity Caster;

    [ExportGroup("Metadata")]

    // Basic ability properties
    [Export]
    public string Name { get; set; } = "Ability";

    [Export] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    // Cooldown and cost properties
    [Export] public double Cooldown { get; set; } = 5.0; // In seconds
    public double CurrentCooldown { get; set; }

    [Export] public AbilityType Type { get; set; } = AbilityType.Active;

    // Toggle state for Toggle abilities
    public bool IsToggled { get; protected set; }


    [ExportGroup("Charged ability properties")]
    // Charging properties for Charged abilities
    [Export]
    public double MaxChargeTime { get; set; } = 1.0;

    public bool AutoCastWhenFull { get; set; } = true;

    [Export] public double MinChargeTime { get; set; } = 0.2;
    public double CurrentChargeTime { get; protected set; }
    public bool IsCharging { get; protected set; }

    // Channeling properties for Channeled abilities
    [ExportGroup("Channeled ability properties")]
    [Export]
    public double ChannelingDuration { get; set; } = 3.0;

    public double CurrentChannelingTime { get; protected set; }
    public bool IsChanneling { get; protected set; }

    // Whether the ability is on cooldown
    public bool IsOnCooldown => CurrentCooldown > 0.0;

    // Whether the ability can be activated
    public virtual bool CanActivate()
    {
        // Check cooldown
        if (IsOnCooldown) return false;

        return Type switch
        {
            // For toggle abilities, we can always toggle
            AbilityType.Toggle => true,
            // For charged abilities, we can start charging
            AbilityType.Charged => !IsCharging,
            // For channeled abilities, we can start channeling
            AbilityType.Channeled => !IsChanneling,
            _ => true
        };
    }

    // Activate the ability
    public virtual void Activate()
    {
        if (!CanActivate()) return;


        // Handle different ability types
        switch (Type)
        {
            case AbilityType.Active:
                ExecuteEffect();
                StartCooldown();
                break;

            case AbilityType.Passive:
                // Passive abilities are always active
                break;

            case AbilityType.Toggle:
                IsToggled = !IsToggled;
                if (IsToggled)
                    OnToggleOn();
                else
                    OnToggleOff();
                break;

            case AbilityType.Charged:
                IsCharging = true;
                CurrentChargeTime = 0.0;
                break;

            case AbilityType.Channeled:
                IsChanneling = true;
                CurrentChannelingTime = 0.0;
                OnChannelingStart();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Update the ability state
    public void UpdateState(double delta)
    {
        // Update cooldown
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= delta;
            if (CurrentCooldown < 0) CurrentCooldown = 0;
        }

        switch (Type)
        {
            // Update charging
            case AbilityType.Charged when !IsCharging:
                return;
            case AbilityType.Charged:
            {
                CurrentChargeTime += delta;
                if (CurrentChargeTime >= MaxChargeTime && AutoCastWhenFull)
                {
                    ReleaseCharge();
                }

                break;
            }

            // Update channeling
            case AbilityType.Channeled when !IsChanneling:
                return;
            case AbilityType.Channeled:
            {
                CurrentChannelingTime += delta;
                OnChannelingTick(delta);

                if (CurrentChannelingTime >= ChannelingDuration)
                {
                    CompleteChanneling();
                }

                break;
            }

            // For toggle abilities, apply effect while toggled
            case AbilityType.Toggle:
            {
                if (IsToggled) OnToggleTick(delta);
                break;
            }

            // For passive abilities, always apply effect
            case AbilityType.Passive:
                OnPassiveTick(delta);
                break;
            case AbilityType.Active:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Update(double delta)
    {
        UpdateState(delta);
    }

    // Release a charged ability
    public virtual void ReleaseCharge()
    {
        if (!IsCharging) return;

        // Calculate charge percentage
        var chargePercentage = (CurrentChargeTime - MinChargeTime) / (MaxChargeTime - MinChargeTime);
        chargePercentage = Mathf.Clamp(chargePercentage, 0.0, 1.0);

        // Execute the ability with charge percentage
        ExecuteEffect(chargePercentage);

        // Reset charging state
        IsCharging = false;
        CurrentChargeTime = 0.0;

        // Start cooldown
        StartCooldown();


        GD.Print($"{Caster.Name} released {Name} with {chargePercentage * 100}% charge");
    }

    // Cancel a charging ability
    public virtual void CancelCharge()
    {
        if (!IsCharging) return;

        // Reset charging state without executing
        IsCharging = false;
        CurrentChargeTime = 0.0;
    }

    // Complete a channeled ability normally
    public virtual void CompleteChanneling()
    {
        if (!IsChanneling) return;

        // Execute final effect
        OnChannelingComplete();

        // Reset channeling state
        IsChanneling = false;
        CurrentChannelingTime = 0.0;

        // Start cooldown
        StartCooldown();
    }

    // Interrupt a channeled ability
    public virtual void InterruptChanneling()
    {
        if (!IsChanneling) return;

        // Execute interrupt effect
        OnChannelingInterrupt();

        // Reset channeling state
        IsChanneling = false;
        CurrentChannelingTime = 0.0;

        // Start a potentially reduced cooldown (50% by default)
        StartCooldown(0.5);
    }

    // Start the ability cooldown
    protected virtual void StartCooldown(double multiplier = 1.0)
    {
        CurrentCooldown = Cooldown * multiplier;
    }

    // Execute the ability effect - to be overridden by derived classes
    protected virtual void ExecuteEffect()
    {
        // Base implementation does nothing
        // Override in derived classes
        GD.Print($"Executing ability {Name}");
    }

    protected virtual void ExecuteEffect(double powerMultiplier)
    {
        // Base implementation does nothing
        // Override in derived classes
        GD.Print($"Executing ability {Name} with power {powerMultiplier}");
    }

    // Toggle ability callbacks
    protected virtual void OnToggleOn()
    {
        GD.Print($"Ability {Name} toggled ON");
    }

    protected virtual void OnToggleOff()
    {
        GD.Print($"Ability {Name} toggled OFF");
    }

    protected virtual void OnToggleTick(double delta)
    {
    }

    // Passive ability callback
    protected virtual void OnPassiveTick(double delta)
    {
    }

    // Channeling ability callbacks
    protected virtual void OnChannelingStart()
    {
        GD.Print($"Started channeling {Name}");
    }

    protected virtual void OnChannelingTick(double delta)
    {
    }

    protected virtual void OnChannelingComplete()
    {
        GD.Print($"Completed channeling {Name}");
    }

    protected virtual void OnChannelingInterrupt()
    {
        GD.Print($"Channeling of {Name} interrupted");
    }
}