using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.LifeCycle;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class EnemyFactory
    {
        private readonly NpcBrain _prefab;
        private readonly Transform[] _waypoints;
        private readonly SpawnService _spawnService;
        private readonly Transform _player;

        public EnemyFactory(NpcBrain prefab, Transform[] waypoints, SpawnService spawnService, Transform player)
        {
            _prefab = prefab;
            _waypoints = waypoints;
            _spawnService = spawnService;
            _player = player;
        }

        public void Create(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            _spawnService.Spawn(_prefab, position, rotation, OnSpawned, parent);
        }

        private void OnSpawned(NpcBrain enemy)
        {
            enemy.Route.Initialize(_waypoints);
            enemy.Initialize(_player);
        }
    }
}