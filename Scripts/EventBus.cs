using System;
using System.Collections.Generic;

namespace ChronosDescent.Scripts;

public enum EventVariant
{
    AbilityChange,
    AbilityStateChange,
    AbilityCooldownChange,
    AbilityCooldownFinished,
    AbilityActivate,

    EffectApplied,
    EffectRemoved,
    EffectRefreshed,
    EffectTimerUpdated,

    EntityDied,
    EntityStatChanged,

    DamageReceived,

    // Currency related events
    CurrencyChanged,
    CurrencyInsufficient
}

public class EventBus
{
    // Dictionary of event types to event instances
    private readonly Dictionary<Type, Dictionary<EventVariant, object>> _events = new();

    // Dictionary to track lambda wrappers for parameterless subscriptions
    private readonly Dictionary<EventVariant, Dictionary<Action, Action<Empty>>> _paramlessWrappers = new();

    // Subscribe to an event
    public void Subscribe<T>(EventVariant ev, Action<T> callback)
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
    public void Subscribe(EventVariant ev, Action callback)
    {
        var eventDict = GetOrCreateEventDictionary<Empty>();

        if (!eventDict.TryGetValue(ev, out var eventObj))
        {
            eventObj = new Event<Empty>();
            eventDict[ev] = eventObj;
        }

        // Create a wrapper that invokes the callback
        Action<Empty> wrapper = _ => callback();

        // Store the wrapper so we can unsubscribe it later
        if (!_paramlessWrappers.TryGetValue(ev, out var wrappers))
        {
            wrappers = new Dictionary<Action, Action<Empty>>();
            _paramlessWrappers[ev] = wrappers;
        }

        // Store the mapping from callback to wrapper
        wrappers[callback] = wrapper;

        // Subscribe the wrapper
        ((Event<Empty>)eventObj).Handler += wrapper;
    }

    // Unsubscribe from an event
    public void Unsubscribe<T>(EventVariant ev, Action<T> callback)
    {
        if (_events.TryGetValue(typeof(T), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<T>)eventObj).Handler -= callback;
    }

    // Unsubscribe from an event without parameters
    public void Unsubscribe(EventVariant ev, Action callback)
    {
        // Get the wrapper for this callback
        if (_paramlessWrappers.TryGetValue(ev, out var wrappers) &&
            wrappers.TryGetValue(callback, out var wrapper))
        {
            // Find the event and remove the wrapper
            if (_events.TryGetValue(typeof(Empty), out var eventDict) &&
                eventDict.TryGetValue(ev, out var eventObj))
                ((Event<Empty>)eventObj).Handler -= wrapper;

            // Remove the wrapper from our tracking dictionary
            wrappers.Remove(callback);

            // Clean up empty dictionaries
            if (wrappers.Count == 0) _paramlessWrappers.Remove(ev);
        }
    }

    // Publish an event with data
    public void Publish<T>(EventVariant ev, T data)
    {
        if (_events.TryGetValue(typeof(T), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<T>)eventObj).Invoke(data);
    }

    // Publish an event without data
    public void Publish(EventVariant ev)
    {
        if (_events.TryGetValue(typeof(Empty), out var eventDict) &&
            eventDict.TryGetValue(ev, out var eventObj))
            ((Event<Empty>)eventObj).Invoke(new Empty());
    }

    // Get or create event dictionary for a type
    private Dictionary<EventVariant, object> GetOrCreateEventDictionary<T>()
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var eventDict)) return eventDict;
        eventDict = new Dictionary<EventVariant, object>();
        _events[type] = eventDict;
        return eventDict;
    }

    // Clear all event handlers
    public void ClearEvents()
    {
        _events.Clear();
        _paramlessWrappers.Clear();
    }

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