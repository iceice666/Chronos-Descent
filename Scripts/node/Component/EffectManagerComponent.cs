using System.Collections.Generic;
using System.Linq;
using Godot;
using ChronosDescent.Scripts.resource;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class EffectManagerComponent : Node
{
    private readonly Dictionary<string, EffectInstance> _activeEffects = new();
    private StatsComponent _stats;

    public void Initialize(StatsComponent statsComponent)
    {
        _stats = statsComponent;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Create a list of effects to remove
        var effectsToRemove = new List<string>();

        // Update all active effects
        foreach (var (key, effectInstance) in _activeEffects)
        {
            effectInstance.Update(delta);

            // Check if effect has expired
            if (effectInstance.IsExpired())
            {
                effectsToRemove.Add(key);
            }
        }

        // Remove expired effects
        foreach (var effectKey in effectsToRemove)
        {
            RemoveEffect(effectKey);
        }
    }

    public void ApplyEffect(Effect effect)
    {
        var effectName = effect.Name;
        var entity = GetOwner<Entity>();


        // Check if the effect is already active
        if (_activeEffects.TryGetValue(effectName, out var existingEffect))
        {
            var currentStacks = existingEffect.CurrentStacks;
            var maxStacks = effect.MaxStacks;

            // If stackable, add a stack
            if (effect.IsStackable && currentStacks < maxStacks)
            {
                existingEffect.AddStack();
                RecalculateStats();

                GD.Print(
                    $"Stacking Effect({effectName}) on Entity({entity.Name}) [{currentStacks + 1}/{maxStacks}]");
            }
            else
            {
                GD.Print($"Refreshing Effect({effectName}) on Entity({entity.Name})");
            }

            // Refresh the duration
            existingEffect.RemainingDuration = effect.Duration;
        }
        // Add new effect
        else
        {
            GD.Print($"Applying Effect({effectName}) on Entity({entity.Name})");

            var newEffect = new EffectInstance(effect, entity);
            _activeEffects[effectName] = newEffect;

            // Apply the effect
            effect.OnApply(entity);

            // Update stats
            RecalculateStats();
        }
    }

    public void RemoveEffect(string effectName)
    {
        if (!_activeEffects.TryGetValue(effectName, out var effectInstance))
            return;
        
        var entity = GetOwner<Entity>();
        
        GD.Print($"Removing Effect({effectName}) to Entity({entity.Name})");

        // Run on-remove callback
        effectInstance.BaseEffect.OnRemove(entity);

        // Remove from active effects
        _activeEffects.Remove(effectName);

        // Recalculate stats
        RecalculateStats();
    }

    public void RemoveAllEffects()
    {
        // Create a copy of the keys to avoid modifying during iteration
        var effectNames = _activeEffects.Keys.ToList();

        foreach (var effectName in effectNames)
        {
            RemoveEffect(effectName);
        }
    }

    public bool HasEffect(string effectName)
    {
        return _activeEffects.ContainsKey(effectName);
    }

    private void RecalculateStats()
    {
        if (_stats == null) return;

        // Reset to base values
        _stats.ResetStatsToBase();

        // Apply all active effects
        foreach (var effectInstance in _activeEffects.Values)
        {
            var effect = effectInstance.BaseEffect;
            var stacks = effectInstance.CurrentStacks;

            // Apply stat modifiers based on number of stacks
            _stats.UpdateStats(stats =>
                {
                    stats.Health += effect.HealthModifier * stacks;
                    stats.Defense += effect.DefenseModifier * stacks;
                    stats.Strength += effect.StrengthModifier * stacks;
                    stats.Intelligence += effect.IntelligenceModifier * stacks;
                    stats.CriticalChance += effect.CriticalChanceModifier * stacks;
                    stats.CriticalDamage += effect.CriticalDamageModifier * stacks;
                    stats.AttackSpeed += effect.AttackSpeedModifier * stacks;
                    stats.MoveSpeed += effect.MoveSpeedModifier * stacks;
                }
            );
        }
    }
}