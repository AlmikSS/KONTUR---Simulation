using _KONTUR___Simulation._Scripts.Input;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.DevConsole
{
    public sealed class DevConsoleAdapter : MonoBehaviour, ITickable
    {
        [SerializeField] private DevConsoleUI _ui;
        
        private InputSystem _inputSystem;
        
        public TickPhase Phase => TickPhase.SimulationPhase;

        public void Initialize()
        {
            ServiceLocator.Get<TickSystem>().Register(this);
            _inputSystem = ServiceLocator.Get<InputSystem>();
        }
        
        public void Tick(float deltaTime)
        {
            if (!_inputSystem.Snapshot.OpenConsole)
                return;
            
            if (!_ui.IsOpened)
            {
                _ui.Open();
                _inputSystem.OpenUI();
            }
            else
            {
                _ui.Close();
                _inputSystem.CloseUI();
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }
    }
}