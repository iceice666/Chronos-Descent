using Godot;

namespace ChronosDescent.Scripts.Ability;

/// <summary>
///     Ability that can be toggled on and off.
/// </summary>
[GlobalClass]
public partial class BaseToggleAbility : BaseAbility
{
    private bool _isToggled;

    public bool IsToggled
    {
        get => _isToggled;
        protected set
        {
            if (_isToggled == value)
                return;
            _isToggled = value;
            OnStateChanged(
                new AbilityStateEventArgs(
                    this,
                    _isToggled ? AbilityState.ToggledOn : AbilityState.ToggledOff
                )
            );
        }
    }

    public override void Activate()
    {
        if (!CanActivate())
            return;

        // Toggle the state
        IsToggled = !IsToggled;

        // Execute effect based on new state
        if (IsToggled)
            OnToggleOn();
        else
            OnToggleOff();

        if (Cooldown > 0)
            StartCooldown();
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        // Continuously update if toggled on
        if (IsToggled)
            OnToggleTick(delta);
    }

    // Toggle callbacks
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
        // Override in derived classes to provide continuous effects while toggled on
    }
}