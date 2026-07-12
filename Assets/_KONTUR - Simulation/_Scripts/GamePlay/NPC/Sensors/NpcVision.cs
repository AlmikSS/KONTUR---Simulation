using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC.Sensors
{
    public sealed class NpcVision : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private Transform _visionOrigin;
        [SerializeField] private LayerMask _visionLayerMask;
        [SerializeField] private float _visionDistance= 20f;
        [SerializeField] private float _visionAngle = 75f;
        [SerializeField] private float _visionCheckInterval = 0.2f;

        private NpcBlackboard _blackboard;
        private Transform _playerTransform;
        private bool _isInit;
        private float _sqrVisionDistance;
        private float _cosVisionAngle;
        private float _visionTimer;
        
        public TickPhase Phase => TickPhase.PostSimulationPhase;

        public void OnSpawn()
        {
            _sqrVisionDistance = _visionDistance * _visionDistance;
            _cosVisionAngle = Mathf.Cos(_visionAngle * 0.5f * Mathf.Deg2Rad);
            _visionTimer = Random.Range(0, _visionCheckInterval);
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            _sqrVisionDistance = 0f;
            _cosVisionAngle = 0f;
            _visionTimer = 0f;
            _isInit = false;
        }
        
        public void Initialize(NpcBlackboard blackboard, Transform playerTransform)
        {
            _blackboard = blackboard;
            _playerTransform = playerTransform;
            _isInit = true;
        }
        
        public void Tick(float deltaTime)
        {
            if (!_isInit)
                return;

            _visionTimer -= deltaTime;
            if (_visionTimer <= 0)
            {
                var isPlayerInVision = IsPlayerInVision();
                _blackboard.SetPlayerPosition(_playerTransform.position, isPlayerInVision);
                _visionTimer = _visionCheckInterval;
            }
        }

        private bool IsPlayerInVision()
        {
            var distanceToPlayer = _playerTransform.position - _visionOrigin.position;
            var sqrDistanceToPlayer = distanceToPlayer.sqrMagnitude;
            
            if (sqrDistanceToPlayer > _sqrVisionDistance)
                return false;
            
            var targetDirection = distanceToPlayer.normalized;
            var dot = Vector3.Dot(_visionOrigin.forward, targetDirection);

            if (dot < _cosVisionAngle)
                return false;

            if (!Physics.Raycast(_visionOrigin.position, targetDirection, out var hit, _visionDistance, _visionLayerMask))
                return false;
            
            return hit.collider.CompareTag("Player");
        }
    }
}