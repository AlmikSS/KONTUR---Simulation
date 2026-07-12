using System;
using System.Collections.Generic;
using _KONTUR___Simulation._Scripts;
using UnityEngine;

namespace KofeyekToolkit.Events
{
    public sealed class EventBus : IService
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

        public void Register<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!_eventHandlers.ContainsKey(type))
                _eventHandlers[type] = new List<Delegate>();

            if (_eventHandlers[type].Contains(handler))
            {
                Debug.LogError($"[EventBus] Try register twice: {type.Name}");
                return;
            }
            
            _eventHandlers[type].Add(handler);
            Debug.Log($"[EventBus] Registered handler: {type.Name}");
        }

        public void Unregister<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
                return;
            
            var type = typeof(T);
            if (!_eventHandlers.TryGetValue(type, out var eventHandler)) 
                return;
            
            eventHandler.Remove(handler);
            if (eventHandler.Count <= 0)
                _eventHandlers.Remove(type);
            
            Debug.Log($"[EventBus] Unregistered handler: {type.Name}");
        }

        public void Invoke<T>(T gameEvent) where T : IGameEvent
        {
            if (gameEvent == null)
                return;
            
            var type = typeof(T);
            if (!_eventHandlers.TryGetValue(type, out var eventHandler))
                return;
            
            var snapshot = eventHandler.ToArray();
            foreach (var handler in snapshot)
            {
                try
                {
                    var action = handler as Action<T>;
                    action?.Invoke(gameEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Failed to invoke handler. Error: { ex.Message }");
                }
            }
        }
    }
}