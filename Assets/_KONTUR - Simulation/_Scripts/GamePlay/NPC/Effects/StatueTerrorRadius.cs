using _KONTUR___Simulation._Scripts.GamePlay.Player;
using _KONTUR___Simulation._Scripts.Services;
using KofeyekToolkit.Events;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    [RequireComponent(typeof(StatueMob))]
    public sealed class StatueTerrorRadius : MonoBehaviour, ITickable
    {
        [Header("Settings")]
        [SerializeField] private float _outerRadius = 15f;
        [SerializeField] private float _innerRadius = 8f;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private Transform _eyeOrigin;

        private EventBus _eventBus;
        private Transform _playerTransform;
        
        private bool _isOuterActive;
        private bool _isInnerActive;

        public TickPhase Phase => TickPhase.SimulationPhase;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _playerTransform = PlayerContext.Transform;
            
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (_playerTransform == null) return;

            float sqrDistance = (_playerTransform.position - transform.position).sqrMagnitude;
            bool hasLineOfSight = HasLineOfSight();

            bool shouldBeOuter = hasLineOfSight && sqrDistance <= _outerRadius * _outerRadius;
            if (shouldBeOuter != _isOuterActive)
            {
                _isOuterActive = shouldBeOuter;
                if (_isOuterActive)
                    _eventBus.Invoke(new TerrorEnterEvent());
                else
                    _eventBus.Invoke(new TerrorLeaveEvent());
            }

            bool shouldBeInner = hasLineOfSight && sqrDistance <= _innerRadius * _innerRadius;
            if (shouldBeInner != _isInnerActive)
            {
                _isInnerActive = shouldBeInner;
                if (_isInnerActive)
                    _eventBus.Invoke(new ChaseStartedEvent());
                else
                    _eventBus.Invoke(new ChaseEndedEvent());
            }
        }

        private bool HasLineOfSight()
        {
            Vector3 targetPos = _playerTransform.position;
            Vector3 origin = _eyeOrigin != null ? _eyeOrigin.position : transform.position;
            Vector3 direction = targetPos - origin;
            float distance = direction.magnitude;

            if (Physics.Raycast(origin, direction, distance, _obstacleLayer))
            {
                return false;
            }

            return true;
        }
    }
}