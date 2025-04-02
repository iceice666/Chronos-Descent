using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;
using Godot.Collections;

namespace ChronosDescent.Scripts.Effects;

public sealed partial class SimpleEffect : BaseEffect
{
    public SimpleEffect(
        string id,
        double duration,
        Dictionary<StatFieldSpecifier, double> additiveModifiers = null
    )
    {
        Id = id;
        Duration = duration;

        if (additiveModifiers != null) AdditiveModifiers = additiveModifiers;
    }

    public override string Id { get; protected set; }
    public override double Duration { get; set; }
}