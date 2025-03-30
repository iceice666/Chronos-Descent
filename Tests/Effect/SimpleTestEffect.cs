using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;

namespace ChronosDescent.Tests.Effect;

public partial class SimpleTestEffect : BaseEffect
{
    public bool OnApplyCalled { get; private set; }
    public bool OnRemoveCalled { get; private set; }


    public override string Id { get; protected  set; } = "simple_test_effect";
    public override double Duration { get;   set; } = 5.0;
    public override string Description { get;  protected set; } = "Simple test effect";

    public override Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get;  protected set; } = new()
    {
        { StatFieldSpecifier.Health, 10.0 }
    };

    public override void OnApply()
    {
        OnApplyCalled = true;
    }

    public override void OnRemove()
    {
        OnRemoveCalled = true;
    }
}