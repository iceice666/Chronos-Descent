using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Tests.Effect;

public class PermanentTestEffect : IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get; set; } = "permanent_test_effect";
    public string Description { get; set; } = "Permanent test effect";
    public Texture2D Icon { get; set; } = null;
    public double Duration { get; set; } = 0.0; // Permanent
    public double TickInterval { get; set; } = 0.0;
    public int MaxStacks { get; set; } = 1;

    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; set; } = new();
    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; set; } = new();

    public void OnApply()
    {
    }

    public void OnRemove()
    {
    }
}