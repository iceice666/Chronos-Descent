using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
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

    #region Helpers

    public bool IsPermanent => Duration <= 0;

    public bool IsStackable => MaxStacks > 0;
    public bool HasStatModifiers => AdditiveModifiers.Count > 0 || MultiplicativeModifiers.Count > 0;
    public bool NeedsTicking => TickInterval > 0;

    #endregion


    #region Callbacks

    public abstract void OnApply();

    public abstract void OnRemove();

    public virtual void OnTick(double delta, int currentStacks)
    {
    }

    public virtual void OnStack(int currentStacks)
    {
    }

    #endregion
}