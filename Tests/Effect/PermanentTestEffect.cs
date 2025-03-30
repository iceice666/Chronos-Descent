using ChronosDescent.Scripts.Core.Effect;

namespace ChronosDescent.Tests.Effect;

public partial class PermanentTestEffect : BaseEffect
{
    public override string Id { get;protected set; } = "permanent_test_effect";
    public override double Duration { get;  set; } = 0;
    public override string Description { get; protected set; } = "Permanent test effect";
}