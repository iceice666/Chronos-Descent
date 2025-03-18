using System.Collections.Generic;
using ChronosDescent.Scripts.Effect.Node;
using Godot;

namespace ChronosDescent.Scripts.Effect.UI;

public partial class EffectsContainer : HBoxContainer
{
    // Dictionary to store active effect indicators
    private readonly Dictionary<string, EffectIndicator> _activeEffects = new();

    // Reference to the effect indicator scene
    private PackedScene _effectIndicatorScene;

    // Reference to the player's effect manager
    public EffectManagerComponent EffectManager { get; private set; }

    public override void _Ready()
    {
        // Load the effect indicator scene
        _effectIndicatorScene = GD.Load<PackedScene>("res://Scenes/ui/effect_indicator.tscn");

        // Set theme constants for proper spacing
        AddThemeConstantOverride("separation", 4);
    }

    public void Initialize(Entity.Entity entity)
    {
        if (entity == null) return;

        EffectManager = entity.EffectManager;

        // Connect signals
        EffectManager.EffectApplied += OnEffectApplied;
        EffectManager.EffectRemoved += OnEffectRemoved;
        EffectManager.EffectTimerUpdated += OnEffectTimerUpdated;
        EffectManager.EffectRefreshed += OnEffectRefreshed;

        GD.Print("Effects container initialized");
    }

    public override void _ExitTree()
    {
        // Disconnect signals when node is removed
        if (EffectManager == null) return;

        EffectManager.EffectApplied -= OnEffectApplied;
        EffectManager.EffectRemoved -= OnEffectRemoved;
        EffectManager.EffectTimerUpdated -= OnEffectTimerUpdated;
        EffectManager.EffectRefreshed -= OnEffectRefreshed;
    }

    private void OnEffectApplied(Effect effect)
    {
        // Create new effect indicator
        var indicator = _effectIndicatorScene.Instantiate<EffectIndicator>();
        AddChild(indicator);

        // Configure the indicator
        indicator.Name = effect.Identifier;
        indicator.Icon = effect.Icon;
        indicator.MaxStack = effect.MaxStacks;
        indicator.RemainingDuration = effect.Duration;

        // Store reference
        _activeEffects[effect.Identifier] = indicator;

        GD.Print($"Effect added to UI: {effect.Name}");
    }

    private void OnEffectRemoved(string effectId)
    {
        if (_activeEffects.TryGetValue(effectId, out var indicator))
        {
            // Remove the indicator
            indicator.QueueFree();
            _activeEffects.Remove(effectId);

            GD.Print($"Effect removed from UI: {effectId}");
        }
    }

    private void OnEffectTimerUpdated(string effectId, double remainingDuration)
    {
        if (_activeEffects.TryGetValue(effectId, out var indicator))
            // Update duration
            indicator.SetDuration(remainingDuration);
    }

    private void OnEffectRefreshed(string effectId, int currentStack)
    {
        if (_activeEffects.TryGetValue(effectId, out var indicator))
            // Update stack count
            indicator.SetStack(currentStack);
    }
}