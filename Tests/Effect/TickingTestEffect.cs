using ChronosDescent.Scripts.Core.Effect;

namespace ChronosDescent.Tests.Effect;

public partial class TickingTestEffect : BaseEffect
{
    public TickingTestEffect()
    {
        Id = "ticking_test_effect";
        Description = "Ticking test effect";
        Duration = 10.0;
        TickInterval = 2.0;
        MaxStacks = 1;
    }

    public int OnTickCalledCount { get; private set; }


    public override void OnTick(double delta, int currentStacks)
    {
        OnTickCalledCount++;
    }
}