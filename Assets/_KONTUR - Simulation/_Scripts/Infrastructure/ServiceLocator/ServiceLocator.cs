using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class, IService
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var existingService))
            {
                if (existingService == null)
                {
                    _services[type] = service;
                    Debug.Log($"[ServiceLocator] Replaced destroyed service {type}");
                    return;
                }

                Debug.LogWarning("[ServiceLocator] Try to register existing service");
                return;
            }
            
            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered service {type}");
        }

        public static void Unregister<T>(T service) where T : class, IService
        {
            var type = typeof(T);
            
            if (!_services.ContainsKey(type))
            {
                Debug.LogWarning("[ServiceLocator] Try to unregister non existing service");
                return;
            }
            
            _services.Remove(type);
            Debug.Log($"[ServiceLocator] Unregistered service {type}");
        }

        public static T Get<T>() where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            
            Debug.LogWarning("[ServiceLocator] Try to get non existing service");
            return null;
        }

        public static void Clear()
        {
            _services.Clear();
            Debug.Log($"[ServiceLocator] All services cleared");
        }
    }
}