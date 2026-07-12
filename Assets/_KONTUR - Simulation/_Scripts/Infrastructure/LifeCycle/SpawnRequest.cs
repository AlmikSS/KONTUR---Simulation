using System;
using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;

namespace KofeyekToolkit.LifeCycle
{
    public struct SpawnRequest<T> : ISpawnRequest where T : Component
    {
        private readonly T _prefab;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Action<T> _onSpawn;
        private readonly Transform _parent;

        public SpawnRequest(T prefab, Vector3 position, Quaternion rotation, Action<T> onSpawn, Transform parent)
        {
            _prefab = prefab;
            _position = position;
            _rotation = rotation;
            _onSpawn = onSpawn;
            _parent = parent;
        }


        public void Execute(SpawnService spawnService)
        {
            var instance = spawnService.ExecutePhysicalSpawn(_prefab, _position, _rotation, _parent);
            _onSpawn?.Invoke(instance);
        }
    }
}