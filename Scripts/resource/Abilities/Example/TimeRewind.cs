using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using Godot;

namespace ChronosDescent.Scripts.resource.Abilities.Example;

[GlobalClass]
public partial class TimeRewindAbility : Ability
{
    private Entity _currentCaster;

    private bool _isRewinding;
    private double _originalHealth;
    private List<TimeManipulationComponent.PositionSpan> _positionHistory = [];
    private bool _processing = true;
    private double _rewindProgress;

    public TimeRewindAbility()
    {
        Name = "Time Rewind";
        Description = "Rewind time to a previous position";
        Type = AbilityType.Active;
        Cooldown = 15.0;
    }

    [Export] public double RewindDuration { get; set; } = 3.0;
    [Export] public double RewindSpeed { get; set; } = 2.0; // Higher means faster rewind
    [Export] public bool HealOnRewind { get; set; } = true;
    [Export] public double HealPercentage { get; set; } = 0.25; // 25% of max health

    public override bool CanActivate()
    {
        if (!base.CanActivate()) return false;

        // Check if Caster has time manipulation component
        var timeManipulation = Caster.TimeManipulation;
        if (timeManipulation == null) return false;

        // Check if there's enough history to rewind
        var history = timeManipulation.GetPositionHistory(RewindDuration).ToList();
        return history.Count > 0;
    }

    public override void Activate()
    {
        if (!CanActivate()) return;


        // Get time manipulation component
        var timeManipulation = Caster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        if (timeManipulation == null) return;

        // Get position history
        _positionHistory = timeManipulation.GetPositionHistory(RewindDuration).ToList();
        if (_positionHistory.Count == 0)
        {
            // Nothing to rewind to
            StartCooldown();
            return;
        }

        // Save current health
        _originalHealth = Caster.Stats.Health;

        // Start rewinding
        _isRewinding = true;
        _rewindProgress = 0.0;
        _currentCaster = Caster;

        // Disable movement control during rewind
        Caster.Moveable = false;

        // Pause recording during rewind
        timeManipulation.PauseRecording();

        // Set process to run the rewind
        _processing = true;

        GD.Print($"{Caster.Name} activated {Name} - rewinding {RewindDuration} seconds");
    }

    public void Process(double delta)
    {
        if (!_isRewinding || _currentCaster == null) return;

        // Update rewind progress
        _rewindProgress += delta * RewindSpeed;

        // Calculate what percentage of the rewind we've completed
        var progress = _rewindProgress / RewindDuration;

        if (progress >= 1.0)
        {
            CompleteRewind();
        }
        else
        {
            // Find the position to interpolate to
            var index = (int)(_positionHistory.Count * progress);
            if (index < _positionHistory.Count) _currentCaster.Position = _positionHistory[index].Position;
        }
    }

    private void CompleteRewind()
    {
        if (_currentCaster == null) return;

        // Set final position
        if (_positionHistory.Count > 0) _currentCaster.Position = _positionHistory[^1].Position;

        // Apply healing if enabled
        if (HealOnRewind)
        {
            var healAmount = _currentCaster.Stats.MaxHealth * HealPercentage;
            _currentCaster.Heal(healAmount);

            GD.Print($"{_currentCaster.Name} healed for {healAmount} after time rewind");
        }

        // Resume time recording
        var timeManipulation = _currentCaster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        timeManipulation?.ResumeRecording();

        // Re-enable movement
        _currentCaster.Moveable = true;

        // Clean up
        _isRewinding = false;
        _currentCaster = null;
        _positionHistory.Clear();

        // Stop processing
        _processing = false;

        // Start cooldown
        StartCooldown();
    }

    // For channeled version, rewind continuously while channeling
    protected override void OnChannelingStart()
    {
        // Get time manipulation component
        var timeManipulation = Caster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        if (timeManipulation == null)
        {
            InterruptChanneling();
            return;
        }

        // Save current position
        _originalHealth = Caster.Stats.Health;
        _currentCaster = Caster;

        // Pause recording during channeling
        timeManipulation.PauseRecording();

        GD.Print($"{Caster.Name} started channeling {Name}");
    }

    protected override void OnChannelingTick(double delta)
    {
        var timeManipulation = Caster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        if (timeManipulation == null) return;

        // Get latest history
        var latestPositions = timeManipulation.GetPositionHistory(delta * RewindSpeed * 2).ToList();
        if (latestPositions.Count == 0) return;

        // Move to the position from a moment ago
        Caster.Position = latestPositions[^1].Position;
    }

    protected override void OnChannelingComplete()
    {
        if (_currentCaster == null) return;

        // Resume time recording
        var timeManipulation = Caster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        timeManipulation?.ResumeRecording();

        // Apply healing if enabled
        if (HealOnRewind)
        {
            var healAmount = Caster.Stats.MaxHealth * HealPercentage;
            Caster.Heal(healAmount);
        }

        // Clean up
        _currentCaster = null;

        GD.Print($"{Caster.Name} completed channeling {Name}");
    }

    protected override void OnChannelingInterrupt()
    {
        // Resume time recording
        var timeManipulation = Caster.GetNode<TimeManipulationComponent>("TimeManipulationComponent");
        timeManipulation?.ResumeRecording();

        // Clean up
        _currentCaster = null;

        GD.Print($"{Caster.Name}'s {Name} channeling was interrupted");
    }
}