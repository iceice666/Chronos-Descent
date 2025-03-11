using ChronosDescent.Scripts.node;

namespace ChronosDescent.Scripts.resource;

public class EffectInstance
{
    private double _timeSinceLastTick;


    public EffectInstance(Effect effect, Entity target)
    {
        effect.Target = target;
        BaseEffect = effect;
        RemainingDuration = effect.Duration;
    }

    public Effect BaseEffect { get; }
    public double RemainingDuration { get; set; }
    public int CurrentStacks { get; private set; } = 1;


    public void Update(double delta)
    {
        if (!BaseEffect.IsPermanent) RemainingDuration -= delta;
    }

    public void UpdateTick(double delta)
    {
        if (!BaseEffect.NeedsTicking) return;

        _timeSinceLastTick += delta;

        if (_timeSinceLastTick < BaseEffect.TickInterval) return;

        if (BaseEffect.IsStackable) BaseEffect.OnTick(_timeSinceLastTick, CurrentStacks);
        else BaseEffect.OnTick(_timeSinceLastTick);

        _timeSinceLastTick = 0;
    }

    public void AddStack()
    {
        if (BaseEffect.IsStackable && CurrentStacks < BaseEffect.MaxStacks)
        {
            CurrentStacks++;
            BaseEffect.OnStack(CurrentStacks);
        }

        // Refresh duration when stacking
        RemainingDuration = BaseEffect.Duration;
    }

    public bool IsExpired()
    {
        return !BaseEffect.IsPermanent && RemainingDuration <= 0;
    }
}