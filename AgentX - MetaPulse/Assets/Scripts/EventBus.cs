using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();

    // Subscribe to an event
    public static void Subscribe<T>(Action<T> handler)
    {
        var eventType = typeof(T);
    
        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = new List<Delegate>();
        }
    
        _eventHandlers[eventType].Add(handler);
    }

    // Unsubscribe from an event
    public static void Unsubscribe<T>(Action<T> handler)
    {
        var eventType = typeof(T);
    
        if (_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType].Remove(handler);
        
            if (_eventHandlers[eventType].Count == 0)
            {
                _eventHandlers.Remove(eventType);
            }
        }
    }

    // Publish an event
    public static void Publish<T>(T eventData)
    {
        var eventType = typeof(T);
    
        if (_eventHandlers.ContainsKey(eventType))
        {
            foreach (var handler in _eventHandlers[eventType].ToArray())
            {
                ((Action<T>)handler).Invoke(eventData);
            }
        }

        // Debug.Log($"Publishing an event of type {eventType}");
    }
}