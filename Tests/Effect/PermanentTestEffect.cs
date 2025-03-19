using ChronosDescent.Scripts.Core.Effect;

namespace ChronosDescent.Tests.Effect;

public partial class PermanentTestEffect : BaseEffect
{
    public PermanentTestEffect()
    {
        Id = "permanent_test_effect";
        Description = "Permanent test effect";
        MaxStacks = 1;
    }
}