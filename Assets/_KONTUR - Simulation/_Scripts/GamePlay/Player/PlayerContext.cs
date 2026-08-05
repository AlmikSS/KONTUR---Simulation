using GamePlay.Player;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player
{
    [SelectionBase]
    public sealed class PlayerContext : MonoBehaviour, IInitializable, IDestroyable, ITickable
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private PlayerStamina _playerStamina;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerState _playerState;
        
        public static Transform Transform { get; private set; }
        public static Camera Camera { get; private set; }
        public static bool CanSprint { get; private set; }
        public static bool IsSprint { get; private set; }
        public static bool IsHidden { get; private set; }

        private static PlayerContext _instance;

        public TickPhase Phase => TickPhase.PostSimulationPhase;
        
        public void OnCreate()
        {
            _instance = this;
            Transform = transform;
            Camera = _playerCamera != null ? _playerCamera : GetComponentInChildren<Camera>();
            
            if (_playerState == null)
                _playerState = GetComponent<PlayerState>();

            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDestroyed()
        {
            if (Transform != transform)
                return;
            
            Transform = null;
            Camera = null;
            _playerState = null;
            _instance = null;
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            IsSprint = _playerMovement.IsSprint;
            CanSprint = _playerStamina.CanSprint;
            IsHidden = _playerState.IsHidden;
        }
    }
}