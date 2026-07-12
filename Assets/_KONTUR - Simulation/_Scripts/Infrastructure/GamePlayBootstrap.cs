using _KONTUR___Simulation._Scripts.DevConsole;
using _KONTUR___Simulation._Scripts.GamePlay.NPC;
using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.LifeCycle;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class GamePlayBootstrap : MonoBehaviour
    {
        [SerializeField] private DevConsoleAdapter _devConsoleAdapter;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private NpcBrain _enemyPrefab;
        [SerializeField] private Transform[] _waypoints;
        
        private SpawnService _spawnService;
        private Transform _playerTransform;
        
        public void Initialize()
        {
            _devConsoleAdapter.Initialize();

            _spawnService = ServiceLocator.Get<SpawnService>();
            _spawnService.Spawn(_playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, o =>
            {
                _playerTransform = o.transform;
                Debug.Log("Player spawned");
            });

            var enemyFactory = new EnemyFactory(_enemyPrefab, _waypoints, _spawnService, _playerTransform);
            enemyFactory.Create(new Vector3(0, 1, 70), Quaternion.identity);
        }
    }
}