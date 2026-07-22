using System.Threading.Tasks;
using _KONTUR___Simulation._Scripts.GamePlay.Interactors;
using _KONTUR___Simulation._Scripts.Input;
using GamePlay.Player;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player
{
    public sealed class PlayerState : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerCamera _playerCamera;

        private InputSystem _inputSystem;
        private ClosetInteractor _closet;

        public TickPhase Phase => TickPhase.SimulationPhase;
        
        public void OnSpawn()
        {
            _inputSystem = ServiceLocator.Get<InputSystem>();
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            _inputSystem = null;
            _closet = null;
        }
        
        public async void Hide(ClosetInteractor interactor)
        {
            _playerMovement.MoveTo(interactor.HidePoint);
            _playerMovement.BlockMovement(true);
            var dir = interactor.LookAtOrigin.position - transform.position;
            dir.y = 0;
            var targetY = Quaternion.LookRotation(dir).eulerAngles.y;
            _playerCamera.LookAt(new  Vector3(transform.eulerAngles.x, targetY, transform.eulerAngles.z));
            await Task.Delay(100);
            _closet = interactor;
        }

        public void Tick(float deltaTime)
        {
            if (_inputSystem.Snapshot.InteractInput && _closet != null)
            {
                _playerMovement.MoveTo(_closet.ExitPoint);
                _playerMovement.BlockMovement(false);
                _closet = null;
            }
        }
    }
}