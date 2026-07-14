using _KONTUR___Simulation._Scripts.DevConsole;
using _KONTUR___Simulation._Scripts.GamePlay.NPC;
using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.LifeCycle;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class GamePlayBootstrap : MonoBehaviour
    {
        [SerializeField] private DevConsoleAdapter _devConsoleAdapter;
        [SerializeField] private NpcBrain _enemyPrefab;
        [SerializeField] private Transform[] _waypoints;
        
        private SpawnService _spawnService;
        
        public void Initialize()
        {
            _devConsoleAdapter.Initialize();
            _spawnService = ServiceLocator.Get<SpawnService>();
            
            var playerTransform = GameObject.FindWithTag("Player").transform;
            var enemyFactory = new EnemyFactory(_enemyPrefab, _waypoints, _spawnService, playerTransform);
            enemyFactory.Create(new Vector3(0, 1, 30), Quaternion.identity);
            
            CommandsRegistry.RegisterAllCommands();
        }
    }
}