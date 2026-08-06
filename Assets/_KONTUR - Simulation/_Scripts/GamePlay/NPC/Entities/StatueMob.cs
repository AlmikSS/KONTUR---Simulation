using _KONTUR___Simulation._Scripts.GamePlay.Player;
using KofeyekToolkit.LifeCycle;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using UnityEngine.AI;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    [SelectionBase]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class StatueMob : MonoBehaviour, ISpawnable, ITickable, IDespawnable
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public NpcConfig Config { get; private set; }

        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private Transform _eyeOrigin;
        [SerializeField] private bool _isDanger;
        [SerializeField] private float _distanceToAction;
        
        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private Camera _playerCamera;

        public TickPhase Phase => TickPhase.SimulationPhase;
        
        public void OnSpawn()
        {
            _agent = GetComponent<NavMeshAgent>();
            _playerTransform = PlayerContext.Transform;
            _playerCamera = PlayerContext.Camera;

            ServiceLocator.Get<TickSystem>().Register(this);

            Animator.Play("Walk", 0f, 0f);
        }

        public void Tick(float deltaTime)
        {
            if (IsVisible())
            {
                _agent.ResetPath();
                _agent.isStopped = true;
            }
            else
            {
                _agent.isStopped = false;
                _agent.SetDestination(_playerTransform.position);
            }

            CheckDistance();
            UpdateAnimation();
        }

        public void OnDespawn()
        {
            _agent = null;
            _playerTransform = null;
            _playerCamera = null;
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }

        private void CheckDistance()
        {
            if (!(Vector3.Distance(_playerTransform.position, transform.position) <= _distanceToAction))
                return;
            if (_isDanger)
            {
                //TODO Game over
            }
            else
            {
                ServiceLocator.Get<SpawnService>().Despawn(gameObject);
            }
        }
        
        private bool IsVisible()
        {
            var screenPoint = _playerCamera.WorldToViewportPoint(transform.position);
            var inViewport = screenPoint is { z: > 0, x: >= 0 and <= 1, y: >= 0 and <= 1 };
            if (!inViewport) return false;
            
            var dirToMob = _eyeOrigin.position - _playerCamera.transform.position;
            if (Physics.Raycast(_playerCamera.transform.position, dirToMob, dirToMob.magnitude, _obstacleLayer))
                return false;

            return true;
        }

        private void UpdateAnimation()
        {
            float speed = Mathf.Clamp01(
                _agent.velocity.magnitude / Mathf.Max(Config.PatrolSpeed, 0.01f));

            Animator.Play("Walk", speed);
        }
    }
}