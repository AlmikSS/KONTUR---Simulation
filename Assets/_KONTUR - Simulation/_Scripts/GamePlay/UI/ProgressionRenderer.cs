using UnityEngine;
using TMPro;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using _KONTUR___Simulation._Scripts.GamePlay.Progression;
using _KONTUR___Simulation._Scripts;

namespace GamePlay.UI
{
    public sealed class ProgressionRenderer : MonoBehaviour, ISpawnable, IDespawnable
    {
        [SerializeField] private TMP_Text _itemsCollectedLabel;
        [SerializeField] private ProgressionCondition _condition;
        [SerializeField] private string _format = "Items Collected: {0}/{1}";
        
        private EventBus _eventBus;
        private ProgressionService _progression;

        public void OnSpawn()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _progression = ServiceLocator.Get<ProgressionService>();
            
            _eventBus.Register<ProgressionStateChangedEvent>(OnProgressionChanged);
            
            UpdateUI();
        }

        public void OnDespawn()
        {
            _eventBus.Unregister<ProgressionStateChangedEvent>(OnProgressionChanged);
            
            _eventBus = null;
            _progression = null;
        }

        private void OnProgressionChanged(ProgressionStateChangedEvent e)
        {
            if (_condition == null)
                return;
            
            if (e.StateKey != _condition.Key)
                return;
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_itemsCollectedLabel == null)
                return;
            
            if (_condition == null)
            {
                _itemsCollectedLabel.text = "Condition not set";
                return;
            }
            
            int current = _progression.GetState(_condition.Key);
            int required = _condition.GetRequiredValue();
            
            _itemsCollectedLabel.text = string.Format(_format, current, required);
        }
    }
}