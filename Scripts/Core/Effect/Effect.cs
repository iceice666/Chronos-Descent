using System;
using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts.Core.Effect;

public interface IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get; protected set; }
    public string Description { get; protected set; }
    public Texture2D Icon { get; protected set; }
    public double Duration { get; protected set; }
    public double TickInterval { get; protected set; }
    public int MaxStacks { get; protected set; }

    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; protected set; }
    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; protected set; }
}

[GlobalClass]
public partial class BaseEffect : Resource, IEffect
{
    public IEntity Owner { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public Texture2D Icon { get; set; }
    public double Duration { get; set; }
    public double TickInterval { get; set; }
    public int MaxStacks { get; set; }
    public Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; set; }
    public Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; set; }

    public BaseEffect()
    {
        AdditiveModifiers = new Dictionary<StatFieldSpecifier, double>();
        MultiplicativeModifiers = new Dictionary<StatFieldSpecifier, double>();
    }

    public virtual void OnApply()
    {
    }

    public virtual void OnRemove()
    {
    }

    public virtual void OnTick(double delta, int currentStacks)
    {
    }

    public virtual void OnStack(int currentStacks)
    {
    }


    #region Helpers

    public bool IsPermanent => Duration <= 0;

    public bool IsStackable => MaxStacks > 0;
    public bool HasStatModifiers => AdditiveModifiers.Count > 0 || MultiplicativeModifiers.Count > 0;
    public bool NeedsTicking => TickInterval > 0;

    #endregion
}