using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Abilities;

public class TimeRewind : BaseChanneledAbility
{
    private const float RewindSpeed = 1.5f; // How quickly we travel through time
    private readonly double _maxRewindDuration = 3.0; // Maximum time we can rewind
    private int _currentHistoryIndex;
    private double _elapsedTime;
    private bool _isRewinding;
    private Vector2 _originalPosition;

    private List<PositionRecord.RecordUnit> _positionHistory;

    // Visual effect properties
    private float _visualEffectIntensity;

    public TimeRewind()
    {
        Description = "Channel to rewind through your past positions";
    }

    public override string Id { get; protected set; } = "time_rewind";
    public override double Cooldown { get; protected init; } = 15.0;
    public override double ChannelingDuration { get; protected set; } = 3.0;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0 && Caster is Player player && player.PositionRecord != null;
    }

    protected override void OnChannelingStart()
    {
        if (Caster is not Player player) return;

        // Store the original position before rewinding
        _originalPosition = player.GlobalPosition;

        // Get position history
        _positionHistory = player.PositionRecord.GetPositionHistory(_maxRewindDuration).ToList();

        // Reverse the history so we move backward in time
        _positionHistory.Reverse();

        // Start from the beginning of history (most recent position)
        _currentHistoryIndex = 0;
        _isRewinding = _positionHistory.Count > 0;
        _elapsedTime = 0.0;

        // Start visual effect
        _visualEffectIntensity = 0.0f;

        // Broadcast start of rewind
        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            "Rewinding Time..."
        );
    }

    protected override void OnChannelingTick(double delta)
    {
        if (!_isRewinding || _positionHistory.Count == 0) return;

        _elapsedTime += delta;

        // Calculate how far we should be in the history based on time and speed
        var targetProgress = (float)(_elapsedTime * RewindSpeed);
        var targetIndex = (int)(targetProgress * _positionHistory.Count);

        // Clamp to valid indices
        targetIndex = Mathf.Clamp(targetIndex, 0, _positionHistory.Count - 1);

        // Only update if we've moved to a new position in history
        if (targetIndex <= _currentHistoryIndex) return;
        _currentHistoryIndex = targetIndex;

        // Move player to the historical position
        var historicalPosition = _positionHistory[_currentHistoryIndex].Position;
        Caster.GlobalPosition = historicalPosition;

        // Increase visual effect intensity as we go further back in time
        _visualEffectIntensity = (float)targetIndex / _positionHistory.Count;
        UpdateVisualEffects();
    }

    protected override void OnChannelingComplete()
    {
        CleanupRewindEffects();
    }

    protected override void OnChannelingInterrupt()
    {
        CleanupRewindEffects();

        // Optionally return to original position if interrupted
        // Caster.GlobalPosition = _originalPosition;

        GlobalEventBus.Instance.Publish(
            GlobalEventVariant.BoardcastTitle,
            "Time Rewind Interrupted"
        );
    }

    private void UpdateVisualEffects()
    {
        // Here we would create or update visual effects based on _visualEffectIntensity
        // For example, post-processing effects, particles, etc.
        // This could include time-related visual cues like ghosting, color shifts, etc.
    }

    private void CleanupRewindEffects()
    {
        _isRewinding = false;
        _visualEffectIntensity = 0.0f;

        // Clean up any visual effects
    }
}