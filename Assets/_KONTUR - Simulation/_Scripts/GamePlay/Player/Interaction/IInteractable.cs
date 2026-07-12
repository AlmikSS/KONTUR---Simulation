using UnityEngine;

namespace GamePlay.Player
{
    public interface IInteractable
    {
        bool HasHint { get; }
        Sprite HintSprite { get; }
        void Interact(GameObject interactor);
    }
}