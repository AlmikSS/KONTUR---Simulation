using _KONTUR___Simulation._Scripts.DevConsole;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class GamePlayBootstrap : MonoBehaviour
    {
        [SerializeField] private DevConsoleAdapter _devConsoleAdapter;
        
        public void Initialize()
        {
            _devConsoleAdapter.Initialize();
        }
    }
}