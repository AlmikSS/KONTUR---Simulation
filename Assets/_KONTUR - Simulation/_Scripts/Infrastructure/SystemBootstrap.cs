using _KONTUR___Simulation._Scripts.Input;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class SystemBootstrap : MonoBehaviour
    {
        [SerializeField] private TickSystem _tickSystem;
        [SerializeField] private InputSystem _inputSystem;
        [SerializeField] private SpawnPoolsConfig _spawnPoolsConfig;
        [SerializeField] private GamePlayBootstrap _gamePlayBootstrap;
        [SerializeField] private Transform _poolsRoot;
        
        private EventBus _eventBus;
        private SpawnService _spawnService;
        
        private void Start()
        {
            _eventBus = new EventBus();
            _spawnService = new SpawnService();
            
            _tickSystem.Initialize();
            _spawnService.Initialize(_spawnPoolsConfig, _poolsRoot);
            _inputSystem.Initialize();
            
            _tickSystem.Register(_spawnService);
            _tickSystem.Register(_inputSystem);
            
            ServiceLocator.Register(_tickSystem);
            ServiceLocator.Register(_spawnService);
            ServiceLocator.Register(_eventBus);
            ServiceLocator.Register(_inputSystem);
            
            CommandsRegistry.RegisterAllCommands();
            _gamePlayBootstrap.Initialize();
            
            _tickSystem.StartTicks();
        }
    }
}