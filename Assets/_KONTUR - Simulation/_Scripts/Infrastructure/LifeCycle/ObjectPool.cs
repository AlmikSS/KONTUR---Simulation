using System;
using System.Collections.Generic;
using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KofeyekToolkit.LifeCycle
{
    public sealed class ObjectPool : IDisposable
    {
        private readonly GameObject _objectPrefab;
        private readonly Queue<GameObject> _poolQueue = new();
        private readonly SpawnService _spawnService;
        private readonly Transform _root;

        public ObjectPool(GameObject objectPrefab, int capacity, SpawnService spawnService, Transform parent = null)
        {
            _objectPrefab = objectPrefab;
            _spawnService = spawnService;
            
            _root = new GameObject($"{_objectPrefab.name}_Pool").transform;
            _root.SetParent(parent);

            for (var i = 0; i < capacity; i++)
            {
                SpawnInstance();
            }
        }
        
        public void Dispose()
        {
            while (_poolQueue.Count > 0)
            {
                var instance = _poolQueue.Dequeue();
                _spawnService.NotifyComponents<IDestroyable>(instance, component => component.OnDestroyed());
            }
            
            Object.Destroy(_root);
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (_poolQueue.Count <= 0)
            {
                SpawnInstance();
            }
            
            var instance = _poolQueue.Dequeue();
            
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.SetParent(parent);
            instance.SetActive(true);
            
            return instance;
        }

        public void Return(GameObject instance)
        {
            instance.transform.SetParent(_root);
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.SetActive(false);
            _poolQueue.Enqueue(instance);
        }
        
        private void SpawnInstance()
        {
            var instance = Object.Instantiate(_objectPrefab, _root);
            _poolQueue.Enqueue(instance);
            instance.SetActive(false);
            _spawnService.NotifyComponents<IInitializable>(instance, component => component.OnCreate());
        }
    }
}