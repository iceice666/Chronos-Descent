using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts.Core.Effect;

public class Manager : ISystem
{
    private readonly Dictionary<string, Instance> _activeEffects = new();
    private readonly List<Instance> _tickingEffects = new();
    private IEntity _owner;
    private bool _statsAreDirty;

    public void Initialize(IEntity owner)
    {
        _owner = owner;
    }

    public void Update(double delta)
    {
    }

    public void FixedUpdate(double delta)
    {
        // Update all effects and identify expired ones
        var effectsToRemove = _activeEffects
            .Where(pair =>
            {
                pair.Value.Update(delta);

                if (!pair.Value.IsExpired())
                    _owner.EventBus.Publish(EventVariant.EffectTimerUpdated, pair.Value.RemainingDuration);

                return pair.Value.IsExpired();
            })
            .Select(pair => pair.Key)
            .ToList();

        // Remove expired effects
        effectsToRemove.ForEach(RemoveEffect);

        // Update ticking effects
        _tickingEffects.ToList().ForEach(effect => effect.UpdateTick(delta));

        if (_statsAreDirty) RecalculateStats();
    }

    private void RecalculateStats()
    {
        Dictionary<StatFieldSpecifier, double> additiveTotal = new();
        Dictionary<StatFieldSpecifier, double> multiplicativeTotal = new();

        // Add up all modifiers from effects
        foreach (var (_, effectInstance) in _activeEffects)
        {
            var effect = effectInstance.BaseEffect;
            var stacks = effectInstance.CurrentStacks;

            if (!effect.HasStatModifiers) continue;

            // Process additive modifiers
            foreach (var (stat, value) in effect.AdditiveModifiers)
            {
                additiveTotal.TryAdd(stat, 0);

                additiveTotal[stat] += value * stacks;
            }

            // Process multiplicative modifiers
            foreach (var (stat, value) in effect.MultiplicativeModifiers)
            {
                multiplicativeTotal.TryAdd(stat, 1.0);

                // Stack with diminishing returns or different formulas if needed
                multiplicativeTotal[stat] *= Mathf.Pow(value, stacks);
            }
        }

        _owner.StatsManager?.Recalculate(additiveTotal, multiplicativeTotal);

        _statsAreDirty = false;
    }


    public void ApplyEffect(BaseEffect effect)
    {
        if (effect == null) return;
        var effectId = effect.Id;

        if (_activeEffects.TryGetValue(effectId, out var existingEffect))
        {
            var currentStacks = existingEffect.CurrentStacks;
            var maxStacks = effect.MaxStacks;

            // If stackable, add a stack
            if (effect.IsStackable && currentStacks < maxStacks)
            {
                existingEffect.AddStack();

                // Mark stats as dirty if this is a stat modifier effect
                if (effect.HasStatModifiers) _statsAreDirty = true;

                // Call the stack callback
                effect.OnStack(currentStacks + 1);
            }
            else
            {
                // Refresh the duration
                existingEffect.RemainingDuration = effect.Duration;
            }

            // Emit signal for UI
            _owner.EventBus.Publish(EventVariant.EffectRefreshed, (effectId, existingEffect.CurrentStacks));
        }
        else
        {
            var newEffectInstance = new Instance(effect, _owner);
            _activeEffects[effectId] = newEffectInstance;

            // Add to specialized lists based on behavior
            if (effect.NeedsTicking) _tickingEffects.Add(newEffectInstance);

            // Apply the effect
            effect.OnApply();

            // Mark stats as dirty if this is a stat modifier effect
            if (effect.HasStatModifiers) _statsAreDirty = true;

            // Emit signal for UI
            _owner.EventBus.Publish(EventVariant.EffectApplied, effect);
        }
    }

    public void RemoveEffect(string effectId)
    {
        if (!_activeEffects.TryGetValue(effectId, out var effectInstance))
            return;

        var effect = effectInstance.BaseEffect;


        // Run on-remove callback
        effect.OnRemove();

        // Remove from specialized lists
        if (effect.NeedsTicking) _tickingEffects.Remove(effectInstance);

        // Remove from active effects
        _activeEffects.Remove(effectId);

        // Mark stats as dirty if this was a stat modifier effect
        if (effect.HasStatModifiers) _statsAreDirty = true;

        // Emit signal for UI
        _owner.EventBus.Publish(EventVariant.EffectRemoved, effectId);
    }

    public bool HasEffect(string effectId)
    {
        return _activeEffects.TryGetValue(effectId, out _);
    }
}

public class Instance
{
    private double _timeSinceLastTick;

    public Instance(
        BaseEffect baseEffect,
        IEntity target,
        int currentStacks = 1)
    {
        RemainingDuration = baseEffect.Duration;
        baseEffect.Owner = target;
        BaseEffect = baseEffect;
        CurrentStacks = currentStacks;
    }

    public BaseEffect BaseEffect { get; }
    public double RemainingDuration { get; set; }
    public int CurrentStacks { get; private set; }

    public void Update(double delta)
    {
        if (!BaseEffect.IsPermanent) RemainingDuration -= delta;
    }


    public void UpdateTick(double delta)
    {
        if (!BaseEffect.NeedsTicking) return;

        _timeSinceLastTick += delta;

        if (_timeSinceLastTick < BaseEffect.TickInterval) return;

        BaseEffect.OnTick(_timeSinceLastTick, CurrentStacks);

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