using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    public class TestInteractable : InteractorBase
    {
        public override void Interact(GameObject interactor)
        {
            Debug.Log("Interacted!");
        }
    }
}