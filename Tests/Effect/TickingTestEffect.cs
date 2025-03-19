using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Tests.Effect;

public class TickingTestEffect : IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get; set; } = "ticking_test_effect";
    public string Description { get; set; } = "Ticking test effect";
    public Texture2D Icon { get; set; } = null;
    public double Duration { get; set; } = 10.0;
    public double TickInterval { get; set; } = 2.0;
    public int MaxStacks { get; set; } = 1;

    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; set; } = new();
    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; set; } = new();

    public int OnTickCalledCount { get; private set; }

    public void OnApply()
    {
    }

    public void OnRemove()
    {
    }

    public void OnTick(double delta, int currentStacks)
    {
        OnTickCalledCount++;
    }
}