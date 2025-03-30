using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;
using Godot.Collections;

namespace ChronosDescent.Tests.Effect;

public partial class StackableTestEffect : BaseEffect
{
    public override string Id { get; protected set; } = "stackable_test_effect";
    public override string Description { get; protected set; } = "Stackable test effect";
    public override double Duration { get; set; } = 10.0;
    public override int MaxStacks { get; protected set; } = 3;

    public override Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; protected set; } = new()
    {
        { StatFieldSpecifier.Health, 5.0 }
    };


    public int OnStackCalledCount { get; private set; }

    public override void OnStack(int currentStacks)
    {
        OnStackCalledCount++;
    }
}