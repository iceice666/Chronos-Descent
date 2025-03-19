using System.Diagnostics;
using ChronosDescent.Scripts;
using ChronosDescent.Scripts.Core.Ability;
using GdUnit4;
using static GdUnit4.Assertions;

namespace ChronosDescent.Tests.Ability;

[TestSuite]
public class AbilityManagerTest
{
    private TestEntity _entity;

    [Before]
    public void TestSetup()
    {
        while (!Debugger.IsAttached) ;
    }

    [BeforeTest]
    public void Setup()
    {
        _entity = new TestEntity();
        _entity._Ready(); // Initialize entity components
    }

    [AfterTest]
    public void Teardown()
    {
        _entity.Free();
    }

    [TestCase]
    public void TestSetAndGetAbility()
    {
        var ability = new MockActiveAbility();
        _entity.SetAbility(AbilitySlotType.Normal, ability);

        var retrievedAbility = _entity.AbilityManager.GetAbility(AbilitySlotType.Normal);

        AssertThat(retrievedAbility).IsEqual(ability);
        AssertThat(retrievedAbility.Caster).IsEqual(_entity);
    }

    [TestCase]
    public void TestRemoveAbility()
    {
        var ability = new MockActiveAbility();
        _entity.SetAbility(AbilitySlotType.Normal, ability);
        _entity.RemoveAbility(AbilitySlotType.Normal);

        var retrievedAbility = _entity.AbilityManager.GetAbility(AbilitySlotType.Normal);

        AssertThat(retrievedAbility).IsNull();
    }

    [TestCase]
    public void TestAbilityChangeEvent()
    {
        var ability = new MockActiveAbility();
        var changeEventFired = false;

        _entity.EventBus.Subscribe<AbilitySlotType>(EventVariant.AbilityChange,
            _ => changeEventFired = true);

        _entity.SetAbility(AbilitySlotType.Normal, ability);

        AssertThat(changeEventFired).IsTrue();
    }

    [TestCase]
    public void TestActivateActiveAbility()
    {
        var ability = new MockActiveAbility();
        var activateEventFired = false;

        _entity.EventBus.Subscribe<BaseAbility>(EventVariant.AbilityActivate,
            _ => activateEventFired = true);

        _entity.SetAbility(AbilitySlotType.Normal, ability);
        _entity.ActivateAbility(AbilitySlotType.Normal);

        AssertThat(activateEventFired).IsTrue();
        AssertThat(ability.ExecuteCalled).IsTrue();
        AssertThat(ability.CurrentCooldown > 0).IsTrue();
    }

    [TestCase]
    public void TestCooldownSystem()
    {
        var ability = new MockActiveAbility();
        var cooldownFinishedEventFired = false;

        _entity.EventBus.Subscribe<BaseAbility>(EventVariant.AbilityCooldownFinished,
            _ => cooldownFinishedEventFired = true);

        _entity.SetAbility(AbilitySlotType.Normal, ability);
        _entity.ActivateAbility(AbilitySlotType.Normal);

        // Simulate time passing
        _entity.AbilityManager.FixedUpdate(3.0); // Exceeds 2.0 cooldown

        AssertThat(cooldownFinishedEventFired).IsTrue();
        AssertThat(ability.CurrentCooldown).IsEqual(0);
    }

    [TestCase]
    public void TestCanActivateAbility()
    {
        var ability = new MockActiveAbility();

        _entity.SetAbility(AbilitySlotType.Normal, ability);
        AssertThat(_entity.CanActivateAbility(AbilitySlotType.Normal)).IsTrue();

        _entity.ActivateAbility(AbilitySlotType.Normal);
        AssertThat(_entity.CanActivateAbility(AbilitySlotType.Normal)).IsFalse();

        // Simulate time passing to finish cooldown
        _entity.AbilityManager.FixedUpdate(3.0);
        AssertThat(_entity.CanActivateAbility(AbilitySlotType.Normal)).IsTrue();
    }

    [TestCase]
    public void TestActivateChanneledAbility()
    {
        var ability = new MockChanneledAbility();

        _entity.SetAbility(AbilitySlotType.Special, ability);
        _entity.ActivateAbility(AbilitySlotType.Special);

        AssertThat(ability.OnChannelingStartCalled).IsTrue();
        AssertThat(_entity.Moveable).IsFalse();
    }

    [TestCase]
    public void TestChanneledAbilityProgress()
    {
        var ability = new MockChanneledAbility();

        _entity.SetAbility(AbilitySlotType.Special, ability);
        _entity.ActivateAbility(AbilitySlotType.Special);

        // Simulate partial channeling time
        ability.Update(1.0);
        AssertThat(ability.OnChannelingTickCalled).IsTrue();
        AssertThat(ability.OnChannelingCompleteCalled).IsFalse();

        // Complete channeling
        ability.Update(2.5); // Total exceeds 3.0 channeling duration
        AssertThat(ability.OnChannelingCompleteCalled).IsTrue();
        AssertThat(ability.CurrentCooldown > 0).IsTrue();
        AssertThat(_entity.Moveable).IsTrue(); // Entity should be movable after completion
    }

    [TestCase]
    public void TestInterruptChanneledAbility()
    {
        var ability = new MockChanneledAbility();

        _entity.SetAbility(AbilitySlotType.Special, ability);
        _entity.ActivateAbility(AbilitySlotType.Special);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot != AbilitySlotType.Unknown).IsTrue();

        _entity.AbilityManager.InterruptAbility(AbilitySlotType.Special);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Unknown).IsTrue();

        AssertThat(ability.OnChannelingInterruptCalled).IsTrue();
        AssertThat(_entity.Moveable).IsTrue();
        AssertThat(ability.CurrentCooldown > 0).IsTrue();
    }

    [TestCase]
    public void TestActivateChargedAbility()
    {
        var ability = new MockChargedAbility();

        _entity.SetAbility(AbilitySlotType.Ultimate, ability);
        _entity.ActivateAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot != AbilitySlotType.Unknown).IsTrue();

        AssertThat(ability.CurrentChargeTime).IsEqual(0);
        AssertThat(_entity.Moveable).IsFalse();
    }

    [TestCase]
    public void TestChargedAbilityProgress()
    {
        var ability = new MockChargedAbility();

        _entity.SetAbility(AbilitySlotType.Ultimate, ability);
        _entity.ActivateAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();

        // Partial charging
        ability.Update(1.0);
        AssertThat(ability.CurrentChargeTime).IsEqual(1.0);
        AssertThat(ability.ExecuteCalled).IsFalse();

        // Ability with AutoCastWhenFull=false shouldn't auto-release
        ability.Update(1.5); // Total exceeds 2.0 max charge time
        AssertThat(ability.ExecuteCalled).IsFalse();
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();
    }

    [TestCase]
    public void TestReleaseChargedAbility()
    {
        var ability = new MockChargedAbility();

        _entity.SetAbility(AbilitySlotType.Ultimate, ability);
        _entity.ActivateAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();

        // Charge enough to meet minimum threshold
        ability.Update(0.6);

        // Release the ability
        _entity.ReleaseAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Unknown).IsTrue();

        AssertThat(ability.ExecuteCalled).IsTrue();
        AssertThat(ability.CurrentCooldown > 0).IsTrue();
        AssertThat(_entity.Moveable).IsTrue();
    }

    [TestCase]
    public void TestCancelChargedAbility()
    {
        var ability = new MockChargedAbility();

        _entity.SetAbility(AbilitySlotType.Ultimate, ability);
        _entity.ActivateAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();

        _entity.CancelAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Unknown).IsTrue();

        AssertThat(ability.OnChargingCanceledCalled).IsTrue();
        AssertThat(_entity.Moveable).IsTrue();
        AssertThat(ability.CurrentChargeTime).IsEqual(0);
    }

    [TestCase]
    public void TestCannotActivateAbilityOnCooldown()
    {
        var ability = new MockActiveAbility();
        var activateCount = 0;

        _entity.EventBus.Subscribe<BaseAbility>(EventVariant.AbilityActivate,
            _ => activateCount++);

        _entity.SetAbility(AbilitySlotType.Normal, ability);
        _entity.ActivateAbility(AbilitySlotType.Normal);
        _entity.ActivateAbility(AbilitySlotType.Normal); // Should not work - on cooldown

        AssertThat(activateCount).IsEqual(1);
    }

    [TestCase]
    public void TestCannotActivateNonexistentAbility()
    {
        var eventFired = false;

        _entity.EventBus.Subscribe<BaseAbility>(EventVariant.AbilityActivate,
            _ => eventFired = true);

        _entity.ActivateAbility(AbilitySlotType.Normal); // No ability in this slot

        AssertThat(eventFired).IsFalse();
    }

    [TestCase]
    public void TestSimultaneousAbilityActivation()
    {
        var chargedAbility = new MockChargedAbility();
        var channeledAbility = new MockChanneledAbility();

        _entity.SetAbility(AbilitySlotType.Ultimate, channeledAbility);
        _entity.SetAbility(AbilitySlotType.Special, chargedAbility);

        // Start charging first
        _entity.ActivateAbility(AbilitySlotType.Ultimate);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();

        // Try to activate another ability (should still work)
        _entity.ActivateAbility(AbilitySlotType.Special);
        AssertThat(_entity.AbilityManager.CurrentActiveAbilitySlot == AbilitySlotType.Ultimate).IsTrue();

        AssertThat(chargedAbility.ExecuteCalled).IsFalse();
    }
}