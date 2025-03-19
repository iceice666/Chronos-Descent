using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Tests.Effect;

public class StackableTestEffect : IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get;  set; } = "stackable_test_effect";
    public string Description { get;  set; } = "Stackable test effect";
    public Texture2D Icon { get;  set; } = null;
    public double Duration { get;  set; } = 10.0;
    public double TickInterval { get;  set; } = 0.0;
    public int MaxStacks { get;  set; } = 3;

    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get;  set; } = new()
    {
        { StatFieldSpecifier.Health, 5.0 }
    };
    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get;  set; } = new();

    public int OnStackCalledCount { get; private set; }

    public void OnApply() { }
    public void OnRemove() { }
    public void OnStack(int currentStacks)
    {
        OnStackCalledCount++;
    }
}