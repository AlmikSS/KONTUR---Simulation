using _KONTUR___Simulation._Scripts.SceneManagement;
using KofeyekToolkit.Events;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.Services
{
    public sealed class PlayerDeathHandler : MonoBehaviour, IService
    {
        private EventBus _eventBus;

        public void Initialize()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<PlayerDiedEvent>(OnPlayerDied);
        }

        public void Shutdown()
        {
            if (_eventBus != null)
            {
                _eventBus.Unregister<PlayerDiedEvent>(OnPlayerDied);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent ev)
        {
            Debug.Log($"[PlayerDeathHandler] Player died from: {ev.DiedFrom}. Restarting scene...");
            
            if (SceneService.Instance != null)
            {
                SceneService.Instance.ReloadScene();
            }
        }
    }
}