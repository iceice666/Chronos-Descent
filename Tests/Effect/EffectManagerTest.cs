using System.Collections.Generic;
using System.Diagnostics;
using ChronosDescent.Scripts;
using ChronosDescent.Scripts.Core.Effect;
using GdUnit4;
using static GdUnit4.Assertions;

namespace ChronosDescent.Tests.Effect;

[TestSuite]
public class EffectManagerTest
{
    private TestEntity _entity;

    [Before]
    public void Setup()
    {
        while (!Debugger.IsAttached) ;
    }

    [BeforeTest]
    public void SetupTest()
    {
        _entity = new TestEntity();
        _entity._Ready();
    }

    [AfterTest]
    public void TeardownTest()
    {
        _entity.Free();
    }

    [TestCase]
    public void TestApplyEffect()
    {
        var effect = new SimpleTestEffect();
        BaseEffect appliedEffect = null;
        _entity.EventBus.Subscribe<BaseEffect>(EventVariant.EffectApplied, e => appliedEffect = e);

        _entity.ApplyEffect(effect);

        AssertThat(effect.OnApplyCalled).IsTrue();
        AssertThat(_entity.HasEffect(effect.Id)).IsTrue();
        AssertThat(appliedEffect).IsEqual(effect);
    }

    [TestCase]
    public void TestEffectExpiration()
    {
        var effect = new SimpleTestEffect();
        var removedEventFired = false;
        string removedEffectId = null;
        _entity.EventBus.Subscribe<string>(EventVariant.EffectRemoved, id =>
        {
            removedEffectId = id;
            removedEventFired = true;
        });

        _entity.ApplyEffect(effect);
        _entity.EffectManager.FixedUpdate(6.0); // Exceeds 5.0 duration

        AssertThat(_entity.HasEffect(effect.Id)).IsFalse();
        AssertThat(removedEventFired).IsTrue();
        AssertThat(removedEffectId).IsEqual(effect.Id);
    }

    [TestCase]
    public void TestStackingEffects()
    {
        var effect = new StackableTestEffect();
        var stackCounts = new List<int>();
        _entity.EventBus.Subscribe<(string, int)>(EventVariant.EffectRefreshed, data =>
        {
            if (data.Item1 == effect.Id) stackCounts.Add(data.Item2);
        });

        _entity.ApplyEffect(effect); // Stack 1
        _entity.ApplyEffect(effect); // Stack 2
        _entity.ApplyEffect(effect); // Stack 3
        _entity.ApplyEffect(effect); // Max stacks, no increase

        AssertThat(stackCounts).ContainsExactly(2, 3, 3);
        AssertThat(effect.OnStackCalledCount).IsEqual(4); // stack 4 times
    }

    [TestCase]
    public void TestDurationRefreshOnReapply()
    {
        var effect1 = new SimpleTestEffect();
        var effect2 = new SimpleTestEffect(); // Same ID

        _entity.ApplyEffect(effect1);
        _entity.EffectManager.FixedUpdate(4.0); // t=4.0
        _entity.ApplyEffect(effect2); // Refreshes duration to 5.0
        _entity.EffectManager.FixedUpdate(2.0); // t=6.0, remaining=3.0

        AssertThat(_entity.HasEffect(effect1.Id)).IsTrue();
    }

    [TestCase]
    public void TestTickingEffect()
    {
        var effect = new TickingTestEffect(); // TickInterval=2.0

        _entity.ApplyEffect(effect);
        _entity.EffectManager.FixedUpdate(1.0); // t=1.0
        AssertThat(effect.OnTickCalledCount).IsEqual(0);

        _entity.EffectManager.FixedUpdate(1.0); // t=2.0
        AssertThat(effect.OnTickCalledCount).IsEqual(1);

        _entity.EffectManager.FixedUpdate(2.0); // t=4.0
        AssertThat(effect.OnTickCalledCount).IsEqual(2);
    }

    [TestCase]
    public void TestPermanentEffect()
    {
        var effect = new PermanentTestEffect();

        _entity.ApplyEffect(effect);
        _entity.EffectManager.FixedUpdate(1000.0); // Long time
        AssertThat(_entity.HasEffect(effect.Id)).IsTrue();

        _entity.RemoveEffect(effect.Id);
        AssertThat(_entity.HasEffect(effect.Id)).IsFalse();
    }
}