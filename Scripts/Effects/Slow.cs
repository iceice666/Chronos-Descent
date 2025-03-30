using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Effects;

[GlobalClass]
public sealed partial class Slow : BaseEffect
{
    private readonly float _slowFactor;

    public Slow(double duration, float slowFactor = 0.5f)
    {
        Duration = duration;
        _slowFactor = slowFactor;

        // Initialize stat modifiers
        MultiplicativeModifiers = new Dictionary<StatFieldSpecifier, double>
        {
            { StatFieldSpecifier.MoveSpeed, _slowFactor }
        };
    }

    public Slow()
    {
    }

    public override string Id { get; protected set; } = "time_slow";
    public override string Description { get; protected set; } = "Slowed by temporal distortion.";

    [Export] public override double Duration { get; set; }
    [Export] public override Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; protected set; }

    public override void OnApply()
    {
        // Visual effect or feedback could be added here
        // e.g., applying a shader to the entity, particle effects, etc.
    }

    public override void OnRemove()
    {
        // Clean up any visual effects when the slow wears off
    }
}