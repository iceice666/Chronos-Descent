using System;
using System.Collections.Generic;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts;

public enum GlobalEventVariant
{
    MagicButtonTriggered,

    DamageDealt,
    EntityDied,
    InputSourceChanged,

    BoardcastTitle,
    RoomStarted,
    RoomCleared,

    // Currency related events
    CurrencyCollected,
    CurrencyDropped,
    ShopTransactionCompleted,
    
    // Game state events
    GameOver
}

[GlobalClass]
public partial class GlobalEventBus : Node
{
    // Dictionary of event types to event instances
    private readonly Dictionary<Type, Dictionary<GlobalEventVariant, object>> _events = new();
    public static GlobalEventBus Instance { get; private set; }

    // Subscribe to an event
    public void Subscribe<T>(GlobalEventVariant ev, Action<T> callback)
    {
        var eventDict = GetOrCreateEventDictionary<T>();

        if (!eventDict.TryGetValue(ev, out var eventObj))
        {
            eventObj = new Event<T>();
            eventDict[ev] = eventObj;
        }

        ((Event<T>)eventObj).Handler += callback;
    }

    // Subscribe to an event without parameters
    public void Subscribe(GlobalEventVariant ev, Action callback)
    {
        var eventDict = GetOrCreateEventDictionary<Empty>();

        if (!eventDict.TryGetValue(ev, out var eventObj))
        {
            eventObj = new Event<Empty>();
            eventDict[ev] = eventObj;
        }

        ((Event<Empty>)eventObj).Handler += _ => callback();
    }

    // Unsubscribe from an event
    public void Unsubscribe<T>(GlobalEventVariant ev, Action<T> callback)
    {
        if (_events.TryGetValue(typeof(T), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<T>)eventObj).Handler -= callback;
    }

    // Unsubscribe from an event without parameters
    public void Unsubscribe(GlobalEventVariant ev, Action callback)
    {
        // This is more complex with this approach and has limitations
    }

    // Publish an event with data
    public void Publish<T>(GlobalEventVariant ev, T data)
    {
        if (_events.TryGetValue(typeof(T), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<T>)eventObj).Invoke(data);
    }

    // Publish an event without data
    public void Publish(GlobalEventVariant ev)
    {
        if (_events.TryGetValue(typeof(Empty), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<Empty>)eventObj).Invoke(new Empty());
    }

    // Get or create event dictionary for a type
    private Dictionary<GlobalEventVariant, object> GetOrCreateEventDictionary<T>()
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var eventDict)) return eventDict;
        eventDict = new Dictionary<GlobalEventVariant, object>();
        _events[type] = eventDict;
        return eventDict;
    }

    // Clear all event handlers
    public void ClearEvents()
    {
        _events.Clear();
    }

    public override void _Ready()
    {
        Instance = this;
        Instance.Subscribe<(double, BaseEntity, EntityStats, Vector2)>(GlobalEventVariant.DamageDealt, OnDamageDealt);
    }


    #region Callbacks

    public void OnDamageDealt((double, BaseEntity, EntityStats, Vector2) data)
    {
        var rawDamage = data.Item1;
        var attackee = data.Item2;
        var attackerStats = data.Item3;
        var rawKb = data.Item4;

        if (attackee == null) return;

        var damage = rawDamage;

        var isCritical = false;
        var critChance = attackerStats?.CriticalChance ?? 0;

        if (GD.Randf() * 100 < critChance)
        {
            // Critical hit applies the critical damage multiplier
            var critMultiplier = attackerStats?.CriticalDamage / 100 ?? 0;
            damage *= 1 + critMultiplier;
            isCritical = true;
        }

        var defenseMultiplier = 100 / (100 + (attackee.StatsManager?.Defense ?? 0));
        damage *= defenseMultiplier;

        var dmgType = isCritical
            ? DamageType.Critical
            : DamageType.Normal;
            
        // Record damage only if attacking an enemy (player is the attacker)
        if (attackee.IsInGroup("Enemy") && !attackee.IsInGroup("Player"))
        {
            GameStats.Instance.RecordDamageCaused(damage);
        }

        attackee.TakeDamage(damage, dmgType, rawKb);
    }

    #endregion

    // Generic event class
    private class Event<T>
    {
        public event Action<T> Handler;

        public void Invoke(T data)
        {
            Handler?.Invoke(data);
        }
    }

    // Empty class for parameterless events
    private struct Empty
    {
    }
}