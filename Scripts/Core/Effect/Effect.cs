
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;
using Godot.Collections;

namespace ChronosDescent.Scripts.Core.Effect;

[GlobalClass]
public abstract partial class BaseEffect : Resource
{
    public BaseEntity Owner { get; set; }
    public abstract string Id { get; protected set; }
    public virtual string Description { get; protected set; }
    public virtual Texture2D Icon { get; protected set; }
    public abstract double Duration { get; set; }
    public virtual double TickInterval { get; protected set; }
    public virtual int MaxStacks { get; protected set; } = 1;
    public virtual Dictionary<StatFieldSpecifier, double> AdditiveModifiers { get; protected set; } = new();
    public virtual Dictionary<StatFieldSpecifier, double> MultiplicativeModifiers { get; protected set; } = new();
    
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
