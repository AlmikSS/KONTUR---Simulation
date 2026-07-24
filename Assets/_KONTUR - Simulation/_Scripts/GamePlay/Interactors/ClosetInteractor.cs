using _KONTUR___Simulation._Scripts.GamePlay.Player;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    [SelectionBase]
    public sealed class ClosetInteractor : InteractorBase
    {
        [SerializeField] private Transform _hidePoint;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _lookAtOrigin;

        public Vector3 HidePoint => _hidePoint.position;
        public Vector3 ExitPoint => _exitPoint.position;
        public Transform LookAtOrigin => _lookAtOrigin;

        public override void Interact(GameObject interactor)
        {
            if (!interactor.TryGetComponent(out PlayerState playerState))
                return;
            
            playerState.Hide(this);
        }
    }
}