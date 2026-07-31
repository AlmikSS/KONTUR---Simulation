using UnityEngine;
using KofeyekToolkit.LifeCycle;
using _KONTUR___Simulation._Scripts.GamePlay.Progression;
using _KONTUR___Simulation._Scripts.GamePlay.Player;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    public class ItemInteractor : InteractorBase
    {
        [Header("Progression")]
        [SerializeField] private string _progressionKey = "items_collected";
        [SerializeField] private int _progressionValue = 1;
        
        private ProgressionService _progression;
        private bool _isCollected;

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            _progression = ServiceLocator.Get<ProgressionService>();
            _isCollected = false;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            
            _progression = null;
        }

        public override void Interact(GameObject interactor)
        {
            if (_isCollected) return;
            
            if (!interactor.TryGetComponent<PlayerState>(out _))
                return;
            
            Collect();
        }

        private void Collect()
        {
            _isCollected = true;
            
            int current = _progression.GetState(_progressionKey);
            _progression.SetState(_progressionKey, current + _progressionValue);
            
            Debug.Log($"Collected {_progressionKey}: {current + _progressionValue}");
            
            ServiceLocator.Get<SpawnService>().Despawn(gameObject);
        }
    }
}