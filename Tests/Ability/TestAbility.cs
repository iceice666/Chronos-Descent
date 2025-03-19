using ChronosDescent.Scripts.Core.Ability;

namespace ChronosDescent.Tests.Ability;

public class MockActiveAbility : BaseActiveAbility
{
    public override string Id { get; protected set; } = "mock_active_ability";
    public MockActiveAbility()
    {
        Description = "A mock active ability for testing";
    }

    public bool ExecuteCalled { get; private set; }

    public override double Cooldown { get; protected init; } = 2.0;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        ExecuteCalled = true;
    }
}

public class MockChanneledAbility : BaseChanneledAbility
{
    public override string Id { get; protected set; } = "mock_channeled_ability";
    public MockChanneledAbility()
    {
        Description = "A mock channeled ability for testing";
        ChannelingDuration = 3.0;
    }

    public bool OnChannelingStartCalled { get; private set; }
    public bool OnChannelingTickCalled { get; private set; }
    public bool OnChannelingCompleteCalled { get; private set; }
    public bool OnChannelingInterruptCalled { get; private set; }

    public override double Cooldown { get; protected init; } = 5.0f;

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        // This is called internally by BaseChanneledAbility's CompleteChanneling method
    }

    protected override void OnChannelingStart()
    {
        OnChannelingStartCalled = true;
    }

    protected override void OnChannelingTick(double delta)
    {
        OnChannelingTickCalled = true;
    }

    protected override void OnChannelingComplete()
    {
        OnChannelingCompleteCalled = true;
    }

    protected override void OnChannelingInterrupt()
    {
        OnChannelingInterruptCalled = true;
    }
}

public class MockChargedAbility : BaseChargedAbility
{
    public override double MinChargeTime { get; set; } = 0.5;
    public override double MaxChargeTime { get; set; } = 2.0;
    public override bool AutoCastWhenFull { get; set; } 
    public override string Id { get; protected set; }= "mock_charged_ability";
    public override double Cooldown { get; protected init; } = 4;

    public MockChargedAbility()
    {
        Description = "A mock charged ability for testing";
    }

    public bool ExecuteCalled { get; private set; }
    public bool OnChargingCanceledCalled { get; private set; }

    public override bool CanActivate()
    {
        return CurrentCooldown <= 0;
    }

    public override void Execute()
    {
        ExecuteCalled = true;
    }


    protected override void OnChargingCanceled()
    {
        OnChargingCanceledCalled = true;
    }
}