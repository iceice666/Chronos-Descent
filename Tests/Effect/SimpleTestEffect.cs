using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Tests.Effect;

public class SimpleTestEffect : IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get; set; } = "simple_test_effect";
    public string Description { get; set; } = "Simple test effect";
    public Texture2D Icon { get; set; } = null;
    public double Duration { get; set; } = 5.0;
    public double TickInterval { get; set; } = 0.0;
    public int MaxStacks { get; set; } = 1;
    
    

    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; set; } = new()
    {
        { StatFieldSpecifier.Health, 10.0 }
    };

    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; set; } = new();

    public bool OnApplyCalled { get; private set; }
    public bool OnRemoveCalled { get; private set; }

    public void OnApply()
    {
        OnApplyCalled = true;
    }

    public void OnRemove()
    {
        OnRemoveCalled = true;
    }
}