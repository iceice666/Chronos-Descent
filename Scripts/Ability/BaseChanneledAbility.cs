using Godot;

namespace ChronosDescent.Scripts.Ability;

/// <summary>
///     Channeled ability that executes continuously for a duration.
/// </summary>
[GlobalClass]
public partial class BaseChanneledAbility : BaseAbility
{
    private bool _isChanneling;

    [ExportGroup("Channeled ability properties")]
    [Export]
    public double ChannelingDuration { get; set; }

    public double CurrentChannelingTime { get; protected set; }

    public bool IsChanneling
    {
        get => _isChanneling;
        protected set
        {
            if (_isChanneling == value)
                return;
            _isChanneling = value;
            OnStateChanged(
                new AbilityStateEventArgs(
                    this,
                    _isChanneling ? AbilityState.Channeling : AbilityState.Default
                )
            );
        }
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && !IsChanneling;
    }

    public override void Activate()
    {
        if (!CanActivate())
            return;

        // Start channeling
        IsChanneling = true;
        CurrentChannelingTime = 0.0;
        OnChannelingStart();
    }

    public sealed override void Update(double delta)
    {
        base.Update(delta);

        if (!IsChanneling)
            return;

        CurrentChannelingTime += delta;
        OnChannelingTick(delta);

        if (CurrentChannelingTime >= ChannelingDuration)
            CompleteChanneling();
    }

    /// <summary>
    ///     Complete a channeled ability normally
    /// </summary>
    public void CompleteChanneling()
    {
        if (!IsChanneling)
            return;

        // Execute final effect
        OnChannelingComplete();

        // Reset channeling state
        IsChanneling = false;
        CurrentChannelingTime = 0.0;

        // Start cooldown
        StartCooldown();
    }

    /// <summary>
    ///     Interrupt a channeled ability
    /// </summary>
    public void InterruptChanneling()
    {
        if (!IsChanneling)
            return;

        // Execute interrupt effect
        OnChannelingInterrupt();

        // Reset channeling state
        IsChanneling = false;
        CurrentChannelingTime = 0.0;

        // Start cooldown
        StartCooldown();
    }

    // Channeling callback methods
    protected virtual void OnChannelingStart()
    {
        GD.Print($"Started channeling {Name}");
    }

    protected virtual void OnChannelingTick(double delta)
    {
        // Override in derived classes to provide continuous effects
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