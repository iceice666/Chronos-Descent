using ChronosDescent.Scripts.Core.Effect;

namespace ChronosDescent.Tests.Effect;

public partial class TickingTestEffect : BaseEffect
{
    public int OnTickCalledCount { get; private set; }
    
    
    public override string Id { get; protected  set; } = "ticking_test_effect";
    public override double Duration { get;   set; } = 10.0;
    public override string Description { get;  protected set; } = "Ticking test effect";
    public override double TickInterval { get;  protected set; } = 2.0;

    public override void OnTick(double delta, int currentStacks)
    {
        OnTickCalledCount++;
    }
}