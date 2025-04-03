using System;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Ability;

public abstract class BaseAbility
{
    #region Fields

    private AbilityState _state;

    public AbilityState State
    {
        get => _state;
        set
        {
            _state = value;
            Caster.EventBus.Publish(EventVariant.AbilityStateChange, this);
        }
    }


    public BaseEntity Caster { get; set; }

    public abstract string Id { get; protected set; }
    public string Description { get; protected set; }
    public Texture2D Icon { get; protected set; }

    private double _currentCooldown;

    public double CurrentCooldown
    {
        get => _currentCooldown;
        protected set
        {
            if (Math.Abs(_currentCooldown - value) <= 0.000001) return;

            _currentCooldown = value;
        }
    }

    public abstract double Cooldown { get; protected init; }

    #endregion

    #region Life Cycle

    public virtual void Initialize(BaseEntity caster)
    {
        Caster = caster;
    }

    public abstract bool CanActivate();
    public abstract void Activate();

    public virtual void Update(double delta)
    {
        UpdateTimer(delta);
    }

    public abstract void Execute();

    #endregion

    #region Helpers

    protected void StartCooldown(double multiplier = 1.0)
    {
        CurrentCooldown = Cooldown * multiplier;
        State = AbilityState.Cooldown;
    }

    protected void UpdateTimer(double delta)
    {
        if (CurrentCooldown <= 0)
            return;

        CurrentCooldown -= delta;
        if (CurrentCooldown < 0)
        {
            CurrentCooldown = 0;
            Caster.EventBus.Publish(EventVariant.AbilityCooldownFinished, this);
        }
        else
        {
            Caster.EventBus.Publish(EventVariant.AbilityCooldownChange, this);
        }
    }

    /// <summary>
    ///     Reduces the cooldown of this ability by the specified amount
    /// </summary>
    /// <param name="amount">Amount to reduce (in seconds)</param>
    public void ReduceCooldown(double amount)
    {
        if (State != AbilityState.Cooldown || amount <= 0)
            return;

        CurrentCooldown = Math.Max(0, CurrentCooldown - amount);

        if (CurrentCooldown <= 0)
        {
            CurrentCooldown = 0;
            State = AbilityState.Idle;
            Caster?.EventBus.Publish(EventVariant.AbilityCooldownFinished, this);
        }
        else
        {
            Caster?.EventBus.Publish(EventVariant.AbilityCooldownChange, this);
        }
    }

    #endregion
}

public abstract class BaseChargedAbility : BaseAbility
{
    public abstract double MinChargeTime { get; set; }
    public abstract double MaxChargeTime { get; set; }

    public abstract bool AutoCastWhenFull { get; set; }


    public double CurrentChargeTime { get; protected set; }


    public override void Activate()
    {
        if (!CanActivate())
            return;

        // Start charging
        State = AbilityState.Executing;
        CurrentChargeTime = 0.0;
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        if (State != AbilityState.Executing) return;

        CurrentChargeTime += delta;

        if (CurrentChargeTime < MaxChargeTime || !AutoCastWhenFull) return;

        ReleaseCharge();
    }

    public void CancelCharge()
    {
        if (State != AbilityState.Executing) return;
        // Reset charging state without executing

        State = AbilityState.Idle;
        CurrentChargeTime = 0.0;
        Caster.Moveable = true;


        OnChargingCanceled();
    }

    public void ReleaseCharge()
    {
        if (State != AbilityState.Executing) return;

        Caster.Moveable = true;

        Execute();
        CurrentChargeTime = 0.0;

        // Start cooldown
        StartCooldown();
    }


    protected abstract void OnChargingCanceled();
}

public abstract class BaseChanneledAbility : BaseAbility
{
    public abstract double ChannelingDuration { get; protected set; }
    public double CurrentChannelingTime { get; protected set; }

    public override void Activate()
    {
        if (!CanActivate())
            return;

        // Start channeling
        State = AbilityState.Executing;
        CurrentChannelingTime = 0.0;
        OnChannelingStart();
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        if (State != AbilityState.Executing) return;

        CurrentChannelingTime += delta;

        if (CurrentChannelingTime >= ChannelingDuration)
            CompleteChanneling();
        else
            OnChannelingTick(delta);
    }

    private void CompleteChanneling()
    {
        if (State != AbilityState.Executing) return;

        Caster.Moveable = true;

        // Execute final effect
        OnChannelingComplete();

        // Reset channeling state
        CurrentChannelingTime = 0.0;

        // Start cooldown
        StartCooldown();
    }

    public void InterruptChanneling()
    {
        if (State != AbilityState.Executing) return;

        Caster.Moveable = true;

        // Execute interrupt effect
        OnChannelingInterrupt();

        // Reset channeling state
        CurrentChannelingTime = 0.0;

        // Start cooldown
        StartCooldown();
    }

    public override void Execute()
    {
        throw new NotImplementedException("How do you reach here?");
    }

    // Channeling callback methods
    protected abstract void OnChannelingStart();
    protected abstract void OnChannelingTick(double delta);
    protected abstract void OnChannelingComplete();
    protected abstract void OnChannelingInterrupt();
}

public abstract class BaseActiveAbility : BaseAbility
{
    public sealed override void Activate()
    {
        if (!CanActivate()) return;

        // Execute effect immediately
        Execute();

        // Start cooldown
        StartCooldown();
    }
}

public abstract class BasePassiveAbility : BaseAbility
{
    protected BasePassiveAbility()
    {
        State = AbilityState.Executing;
    }

    public override void Update(double delta)
    {
        OnPassiveTick(delta);
    }

    protected abstract void OnPassiveTick(double delta);
}

public enum AbilityState
{
    Idle,
    Executing,
    Cooldown
}