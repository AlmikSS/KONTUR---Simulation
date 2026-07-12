using System;
using System.Collections.Generic;
using _KONTUR___Simulation._Scripts;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KofeyekToolkit.LifeCycle
{
    public sealed class SpawnService : ITickable, IService
    {
        private readonly Dictionary<EntityId, ObjectPool> _pools = new();
        private readonly Queue<ISpawnRequest> _spawnQueue = new();
        private readonly Queue<GameObject> _despawnQueue = new();

        public TickPhase Phase => TickPhase.SpawnDespawnPhase;
        
        internal void Initialize(SpawnPoolsConfig poolsConfig, Transform poolsRoot = null)
        {
            foreach (var config in poolsConfig.Pools)
            {
                var id = config.Prefab.gameObject.GetEntityId();
                var pool = new ObjectPool(config.Prefab, config.StartPoolSize, this, poolsRoot);
                _pools.Add(id, pool);
            }
        }

        public void Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Action<T> action, Transform parent = null) where T : Component
        {
            var request = new SpawnRequest<T>(prefab, position, rotation, action, parent);
            _spawnQueue.Enqueue(request);
        }

        public void Despawn(GameObject instance)
        {
            _despawnQueue.Enqueue(instance);
        }
        
        public void Tick(float deltaTime)
        {
            while (_spawnQueue.Count > 0)
            {
                var request = _spawnQueue.Dequeue();
                if (request != null)
                    request.Execute(this);
            }

            while (_despawnQueue.Count > 0)
            {
                var instance = _despawnQueue.Dequeue();
                var id = instance.GetEntityId();
                if (_pools.TryGetValue(id, out var pool))
                {
                    pool.Return(instance);
                    continue;
                }
                
                NotifyComponents<IDestroyable>(instance, component => component.OnDestroyed());
                Object.Destroy(instance);
            }
        }

        internal T ExecutePhysicalSpawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            var id = prefab.gameObject.GetEntityId();
            GameObject instance = null;

            if (_pools.TryGetValue(id, out var pool))
            {
                instance = pool.Get(position, rotation, parent);
            }
            else
            {
                instance = Object.Instantiate(prefab, position, rotation, parent).gameObject;
            }
            
            NotifyComponents<ISpawnable>(instance, component => component.OnSpawn());
            return  instance.GetComponent<T>();
        }
        
        internal void NotifyComponents<TInterface>(GameObject target, Action<TInterface> action) where TInterface : class
        {
            var components = target.GetComponents<TInterface>();
            
            foreach (var component in components)
            {
                action(component);
            }
        }
    }
}