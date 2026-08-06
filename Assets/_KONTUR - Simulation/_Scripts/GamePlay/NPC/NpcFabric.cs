using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.LifeCycle;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class NpcFabric : MonoBehaviour
    {
        [SerializeField] private Transform _spawnpoint;
        [SerializeField] private Transform _parent;
        [SerializeField] private NpcBrain _prefab;
        [SerializeField] private Transform[] _waypoints;

        public void Spawn()
        {
            ServiceLocator.Get<SpawnService>().Spawn(_prefab, _spawnpoint.position, Quaternion.identity,
                brain =>
                {
                    brain.Route.Initialize(_waypoints);
                    brain.Initialize();
                }, _parent);
        }
    }
}