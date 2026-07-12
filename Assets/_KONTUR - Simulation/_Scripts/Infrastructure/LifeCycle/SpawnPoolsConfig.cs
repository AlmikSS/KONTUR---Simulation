using System;
using System.Collections.Generic;
using UnityEngine;

namespace KofeyekToolkit.LifeCycle
{
    [CreateAssetMenu(menuName = "KofeyekToolkit/SpawnPoolsConfig")]
    public sealed class SpawnPoolsConfig : ScriptableObject
    {
        [SerializeField] private List<PoolConfig> _pools = new();
        
        public IReadOnlyList<PoolConfig> Pools => _pools;
    }

    [Serializable]
    public class PoolConfig
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _startPoolSize;

        public GameObject Prefab => _prefab;
        public int StartPoolSize => _startPoolSize;
    }
}