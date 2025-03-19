using ChronosDescent.Scripts.Core.Ability;

namespace ChronosDescent.Tests;

public class MockActiveAbility : BaseActiveAbility
{
    public bool ExecuteCalled { get; private set; }
    
    public MockActiveAbility()
    {
        Id = "mock_active_ability";
        Description = "A mock active ability for testing";
        Cooldown = 2.0;
    }
    
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
    public bool OnChannelingStartCalled { get; private set; }
    public bool OnChannelingTickCalled { get; private set; }
    public bool OnChannelingCompleteCalled { get; private set; }
    public bool OnChannelingInterruptCalled { get; private set; }
    
    public MockChanneledAbility()
    {
        Id = "mock_channeled_ability";
        Description = "A mock channeled ability for testing";
        Cooldown = 5.0;
        ChannelingDuration = 3.0;
    }
    
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
    public bool ExecuteCalled { get; private set; }
    public bool OnChargingCanceledCalled { get; private set; }
    
    public MockChargedAbility()
    {
        Id = "mock_charged_ability";
        Description = "A mock charged ability for testing";
        Cooldown = 4.0;
        MaxChargeTime = 2.0;
        MinChargeTime = 0.5;
        AutoCastWhenFull = false;
    }
    
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