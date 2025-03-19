using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.Stats;
using Godot;

namespace ChronosDescent.Scripts;

[GlobalClass]
public partial class GlobalEventBus : EventBus
{
    public static GlobalEventBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public GlobalEventBus()
    {
        Subscribe<(double, IEntity, EntityStats)>(EventVariant.DamageDealt, OnDamageDealt);
    }

    #region Observers

    public void OnDamageDealt((double, IEntity, EntityStats) data)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}