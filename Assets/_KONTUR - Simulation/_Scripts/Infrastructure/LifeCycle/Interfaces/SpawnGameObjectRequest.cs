using System;
using UnityEngine;

namespace KofeyekToolkit.LifeCycle.Interfaces
{
    internal struct SpawnGameObjectRequest : ISpawnRequest
    {
        private readonly GameObject _prefab;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Transform _parent;
        private readonly Action<GameObject> _onSpawned;

        public SpawnGameObjectRequest(GameObject prefab, Vector3 position, Quaternion rotation, Action<GameObject> onSpawned, Transform parent)
        {
            _prefab = prefab;
            _position = position;
            _rotation = rotation;
            _parent = parent;
            _onSpawned = onSpawned;
        }

        public void Execute(SpawnService spawnService)
        {
            var instance = spawnService.ExecutePhysicalSpawnGameObject(_prefab, _position, _rotation, _parent);
            _onSpawned?.Invoke(instance);
        }
    }
}