using GamePlay.Player;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    public abstract class InteractorBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private Sprite _hint;
        
        public bool HasHint => _hint != null;
        public Sprite HintSprite => _hint;
        
        public virtual void Interact(GameObject interactor) { }

        public virtual void SecondaryInteract(GameObject interactor) { }
    }
}