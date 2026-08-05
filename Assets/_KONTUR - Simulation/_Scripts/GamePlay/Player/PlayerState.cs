using System.Threading;
using System.Threading.Tasks;
using _KONTUR___Simulation._Scripts.GamePlay.Interactors;
using _KONTUR___Simulation._Scripts.Input;
using GamePlay.Player;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player
{
    public sealed class PlayerState : MonoBehaviour, ISpawnable, IDespawnable
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerCamera _playerCamera;

        [Header("Hiding Detection")]
        [SerializeField] private LayerMask _hidingSpotLayer;
        [SerializeField] private float _checkDistance = 2f;

        private EventBus _eventBus;
        private InputSystem _inputSystem;
        private ClosetInteractor _closet;
        private CancellationTokenSource _hideCts;

        public TickPhase Phase => TickPhase.SimulationPhase;
        public bool IsHidden => _closet != null || CheckIsUnderHidingRoof() == true;

        public void OnSpawn()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _inputSystem = ServiceLocator.Get<InputSystem>();
        }

        public void OnDespawn()
        {
            _hideCts?.Cancel();
            _inputSystem = null;
            _closet = null;
        }

        private void OnDestroy()
        {
            _hideCts?.Cancel();
        }

        public bool IsHiddenIn(ClosetInteractor interactor) => _closet == interactor;

        public async void Hide(ClosetInteractor interactor)
        {
            if (IsHidden) return;

            _eventBus.Invoke(new ShelterEnterEvent());

            _hideCts = new CancellationTokenSource();
            var token = _hideCts.Token;

            _playerMovement.MoveTo(interactor.HidePoint);
            _playerMovement.BlockMovement(true);
            var dir = interactor.LookAtOrigin.position - transform.position;
            dir.y = 0;
            var targetY = Quaternion.LookRotation(dir).eulerAngles.y;
            _playerCamera.LookAt(new Vector3(transform.eulerAngles.x, targetY, transform.eulerAngles.z));

            try
            {
                await Task.Delay(100, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            _closet = interactor;
        }

        public bool CheckIsUnderHidingRoof()
        {
            var rayOrigin = transform.position + Vector3.up * 0.1f;
            
            if (Physics.Raycast(rayOrigin, Vector3.up, out var hit, _checkDistance, _hidingSpotLayer))
            {
                return true;
            }
            
            return false;
        }

        public void ExitHiding()
        {
            if (!IsHidden) return;

            _eventBus.Invoke(new ShelterLeaveEvent());

            if (_closet != null)
            {
                _playerMovement.MoveTo(_closet.ExitPoint);
                _closet = null;
            }
            
            _playerMovement.BlockMovement(false);
        }
    }
}