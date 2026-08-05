using KofeyekToolkit.Events;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _KONTUR___Simulation._Scripts.Services
{
    public sealed class FallDamageHandler : MonoBehaviour, IService
    {
        [Header("Fall Death Settings")]
        [SerializeField] private float _fatalFallHeight = 10f;
        [SerializeField] private float _cooldown = 0.5f;

        private EventBus _eventBus;
        private float _lastEventTime;

        public TickPhase Phase => TickPhase.SimulationPhase;

        public void Initialize()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<PlayerLandedEvent>(OnPlayerLanded);
        }

        public void Shutdown()
        {
            _eventBus.Unregister<PlayerLandedEvent>(OnPlayerLanded);
            _eventBus = null;
        }

        private void OnPlayerLanded(PlayerLandedEvent ev)
        {
            if (Time.time - _lastEventTime < _cooldown)
                return;

            if (ev.Distance >= _fatalFallHeight)
            {
                _lastEventTime = Time.time;
                _eventBus.Invoke(new PlayerDiedEvent(PlayerDiedFromType.Fall));
            }
        }
    }
}