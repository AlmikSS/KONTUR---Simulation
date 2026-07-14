using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    public sealed class ProgressionReactor : MonoBehaviour, IInitializable, IDestroyable
    {
        [SerializeField] private ProgressionCondition _condition;
        [SerializeField] private UnityEvent _onConditionCompleted;
        
        private EventBus _eventBus;
        
        public void OnCreate()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<ProgressionStateChangedEvent>(OnProgressionStateChanged);
        }

        public void OnDestroyed()
        {
            _eventBus = null;
        }

        private void OnProgressionStateChanged(ProgressionStateChangedEvent e)
        {
            if (e.StateKey != _condition.Key)
                return;

            if (_condition.IsSatisfied(e.ProgressionService))
                _onConditionCompleted.Invoke();
        }
    }
}