using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;

namespace ChronosDescent.Tests.Effect;

public partial class SimpleTestEffect : BaseEffect
{
    public SimpleTestEffect()
    {
        Id = "simple_test_effect";
        Description = "Simple test effect";
        Duration = 5.0;
        MaxStacks = 1;
        AdditiveModifiers.Add(StatFieldSpecifier.Health, 10.0);
    }

    public bool OnApplyCalled { get; private set; }
    public bool OnRemoveCalled { get; private set; }


    public override void OnApply()
    {
        OnApplyCalled = true;
    }

    public override void OnRemove()
    {
        OnRemoveCalled = true;
    }
}