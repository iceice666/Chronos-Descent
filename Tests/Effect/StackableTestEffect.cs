using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.State;

namespace ChronosDescent.Tests.Effect;

public partial class StackableTestEffect : BaseEffect
{
    public StackableTestEffect()
    {
        Id = "stackable_test_effect";
        Description = "Stackable test effect";
        Duration = 10.0;
        MaxStacks = 3;
        AdditiveModifiers.Add(StatFieldSpecifier.Health, 5.0);
    }

    public int OnStackCalledCount { get; private set; }

    public override void OnStack(int currentStacks)
    {
        OnStackCalledCount++;
    }
}