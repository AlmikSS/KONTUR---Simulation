using _KONTUR___Simulation._Scripts.DevConsole;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.LifeCycle;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class GamePlayBootstrap : MonoBehaviour
    {
        [SerializeField] private DevConsoleAdapter _devConsoleAdapter;
        [SerializeField] private Transform[] _waypoints;
        
        private SpawnService _spawnService;
        
        public void Initialize()
        {
            _devConsoleAdapter.Initialize();
            CommandsRegistry.RegisterAllCommands();
        }
    }
}