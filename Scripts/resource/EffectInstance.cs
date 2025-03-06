using ChronosDescent.Scripts.node;

namespace ChronosDescent.Scripts.resource;

using Godot;

// An instance of an effect applied to an entity
public partial class EffectInstance : RefCounted
{
    public Effect BaseEffect { get; private set; }
    public double RemainingDuration { get; set; }
    public int CurrentStacks { get; private set; } = 1;
    public double NextTickTime { get; set; }
    public Entity Target { get; private set; }

    public EffectInstance(Effect effect, Entity target)
    {
        BaseEffect = effect;
        Target = target;
        RemainingDuration = effect.Duration;
        NextTickTime = effect.TickInterval;
    }

    public EffectInstance()
    {
    }

    public void Update(double delta)
    {
        
        
        if (!BaseEffect.IsPermanent)
        {
            RemainingDuration -= delta;
        }
        
        // Handle ticking effects (like DOT or HOT)
        if (BaseEffect.TickInterval <= 0) return;

        NextTickTime -= delta;
        if (NextTickTime > 0) return;

        NextTickTime = BaseEffect.TickInterval;
        BaseEffect.OnTick(Target);
    }


    public void AddStack()
    {
        if (BaseEffect.IsStackable && CurrentStacks < BaseEffect.MaxStacks)
        {
            CurrentStacks++;
            BaseEffect.OnStack(Target, CurrentStacks);
        }

        // Refresh duration when stacking
        RemainingDuration = BaseEffect.Duration;
    }

    public bool IsExpired()
    {
        return !BaseEffect.IsPermanent && RemainingDuration <= 0;
    }
}