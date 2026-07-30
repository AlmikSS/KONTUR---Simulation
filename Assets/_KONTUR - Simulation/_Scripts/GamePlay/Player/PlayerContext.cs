using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player
{
    [SelectionBase]
    public sealed class PlayerContext : MonoBehaviour, IInitializable, IDestroyable
    {
        [SerializeField] private Camera _playerCamera;
        
        public static Transform Transform { get; private set; }
        public static Camera Camera { get; private set; }
        
        public void OnCreate()
        {
            Transform = transform;
            Camera = _playerCamera != null ? _playerCamera : GetComponentInChildren<Camera>();
        }

        public void OnDestroyed()
        {
            if (Transform != transform)
                return;
            
            Transform = null;
            Camera = null;
        }
    }
}