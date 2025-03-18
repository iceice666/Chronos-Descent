using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using BaseStats = ChronosDescent.Scripts.Entity.Resource.BaseStats;
using StatsComponent = ChronosDescent.Scripts.Entity.Node.StatsComponent;

namespace ChronosDescent.Scripts.Effect.Node;

[GlobalClass]
public partial class EffectManagerComponent : Godot.Node
{
    // Signal for UI updates
    public delegate void EffectAppliedEventHandler(Effect effect);

    public delegate void EffectRefreshedEventHandler(string effectId, int currentStack);

    public delegate void EffectRemovedEventHandler(string effectId);

    public delegate void EffectTimerUpdatedEventHandler(string effectId, double remainingDuration);


    private readonly Dictionary<string, EffectInstance> _activeEffects = new();
    private readonly List<EffectInstance> _controlEffects = [];

    // Lists for optimization
    private readonly List<EffectInstance> _tickingEffects = [];

    private Entity.Entity _entity;
    private StatsComponent _stats;
    private bool _statsAreDirty;

    public event EffectAppliedEventHandler EffectApplied;
    public event EffectRefreshedEventHandler EffectRefreshed;
    public event EffectRemovedEventHandler EffectRemoved;
    public event EffectTimerUpdatedEventHandler EffectTimerUpdated;

    protected virtual void OnEffectApplied(Effect effect)
    {
        EffectApplied?.Invoke(effect);
    }

    protected virtual void OnEffectRefreshed(string effectId, int currentStack)
    {
        EffectRefreshed?.Invoke(effectId, currentStack);
    }

    protected virtual void OnEffectRemoved(string effectId)
    {
        EffectRemoved?.Invoke(effectId);
    }

    protected virtual void OnEffectTimerUpdated(string effectId, double remainingDuration)
    {
        EffectTimerUpdated?.Invoke(effectId, remainingDuration);
    }

    public void Initialize(StatsComponent statsComponent)
    {
        _stats = statsComponent;
        _entity = GetOwner<Entity.Entity>();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Create a list of effects to remove
        var effectsToRemove = new List<string>();

        // Update duration on all effects
        foreach (var (effectId, effectInstance) in _activeEffects)
        {
            effectInstance.Update(delta);

            // Check if the effect has expired
            if (effectInstance.IsExpired())
                effectsToRemove.Add(effectId);
            else
                // Emit update signal for UI
                OnEffectTimerUpdated(effectInstance.BaseEffect.Identifier,
                    effectInstance.RemainingDuration);
        }

        // Remove expired effects
        foreach (var effectId in effectsToRemove) RemoveEffect(effectId);

        // Update only effects that need ticking
        foreach (var effect in _tickingEffects.ToList()) effect.UpdateTick(delta);

        // Only recalculate stats when necessary
        if (_statsAreDirty)
        {
            RecalculateStats();
            _statsAreDirty = false;
        }
    }

    public void ApplyEffect(Effect effect)
    {
        if (effect == null) return;

        var effectId = effect.Identifier;


        // Check if already active
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

                // Emit signal for UI
                OnEffectRefreshed(effectId, currentStacks + 1);

                GD.Print($"Stacking Effect({effectId}) on Entity({_entity.Name}) [{currentStacks + 1}/{maxStacks}]");
            }
            else
            {
                // Refresh the duration
                existingEffect.RemainingDuration = effect.Duration;

                // Emit signal for UI
                OnEffectRefreshed(effectId, currentStacks);

                GD.Print($"Refreshing Effect({effectId}) on Entity({_entity.Name})");
            }
        }
        // Add new effect
        else
        {
            GD.Print($"Applying Effect({effectId}) on Entity({_entity.Name})");

            var newEffectInstance = new EffectInstance(effect, _entity);
            _activeEffects[effectId] = newEffectInstance;

            // Add to specialized lists based on behavior
            if (effect.NeedsTicking) _tickingEffects.Add(newEffectInstance);

            if (effect.IsControlEffect) _controlEffects.Add(newEffectInstance);

            // Apply the effect
            effect.OnApply();

            // Mark stats as dirty if this is a stat modifier effect
            if (effect.HasStatModifiers) _statsAreDirty = true;

            // Emit signal for UI
            OnEffectApplied(effect);
        }
    }

    public void RemoveEffect(string effectId)
    {
        if (!_activeEffects.TryGetValue(effectId, out var effectInstance))
            return;

        var effect = effectInstance.BaseEffect;

        GD.Print($"Removing Effect({effectId}) from Entity({_entity.Name})");

        // Run on-remove callback
        effect.OnRemove();

        // Remove from specialized lists
        if (effect.NeedsTicking) _tickingEffects.Remove(effectInstance);

        if (effect.IsControlEffect) _controlEffects.Remove(effectInstance);

        // Remove from active effects
        _activeEffects.Remove(effectId);

        // Mark stats as dirty if this was a stat modifier effect
        if (effect.HasStatModifiers) _statsAreDirty = true;

        // Emit signal for UI
        OnEffectRemoved(effectId);
    }

    public void RemoveAllEffects()
    {
        // Create a copy of the keys to avoid modification during iteration
        var effectIds = _activeEffects.Keys.ToList();

        foreach (var effectId in effectIds) RemoveEffect(effectId);
    }

    public bool HasEffect(string effectId)
    {
        return _activeEffects.ContainsKey(effectId);
    }

    public int GetEffectStacks(string effectId)
    {
        return _activeEffects.TryGetValue(effectId, out var effect) ? effect.CurrentStacks : 0;
    }

    public List<Effect> GetAllActiveEffects()
    {
        return _activeEffects.Values.Select(instance => instance.BaseEffect).ToList();
    }

    public void SetStatsDirty()
    {
        _statsAreDirty = true;
    }

    public bool HasControlEffect()
    {
        return _controlEffects.Count > 0;
    }

    private void RecalculateStats()
    {
        if (_stats == null) return;

        // Reset to base values
        _stats.ResetStatsToBase();

        // First pass: collect all modifiers
        Dictionary<BaseStats.Specifier, double> additiveTotal = new();
        Dictionary<BaseStats.Specifier, double> multiplicativeTotal = new();

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

        // Second pass: apply all modifiers
        var stats = _entity.Stats;

        foreach (var (stat, value) in multiplicativeTotal)
            switch (stat)
            {
                case BaseStats.Specifier.Health:
                    stats.Health *= value;
                    break;
                case BaseStats.Specifier.MaxHealth:
                    stats.MaxHealth *= value;
                    break;
                case BaseStats.Specifier.CurrentResource:
                    stats.CurrentResource *= value;
                    break;
                case BaseStats.Specifier.MaxResource:
                    stats.MaxResource *= value;
                    break;
                case BaseStats.Specifier.Defense:
                    stats.Defense *= value;
                    break;
                case BaseStats.Specifier.CriticalChance:
                    stats.CriticalChance *= value;
                    break;
                case BaseStats.Specifier.CriticalDamage:
                    stats.CriticalDamage *= value;
                    break;
                case BaseStats.Specifier.AttackSpeed:
                    stats.AttackSpeed *= value;
                    break;
                case BaseStats.Specifier.MoveSpeed:
                    stats.MoveSpeed *= value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        foreach (var (stat, value) in additiveTotal)
            switch (stat)
            {
                case BaseStats.Specifier.Health:
                    stats.Health += value;
                    break;
                case BaseStats.Specifier.MaxHealth:
                    stats.MaxHealth += value;
                    break;
                case BaseStats.Specifier.CurrentResource:
                    stats.CurrentResource += value;
                    break;
                case BaseStats.Specifier.MaxResource:
                    stats.MaxResource += value;
                    break;
                case BaseStats.Specifier.Defense:
                    stats.Defense += value;
                    break;
                case BaseStats.Specifier.CriticalChance:
                    stats.CriticalChance += value;
                    break;
                case BaseStats.Specifier.CriticalDamage:
                    stats.CriticalDamage += value;
                    break;
                case BaseStats.Specifier.AttackSpeed:
                    stats.AttackSpeed += value;
                    break;
                case BaseStats.Specifier.MoveSpeed:
                    stats.MoveSpeed += value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }
}