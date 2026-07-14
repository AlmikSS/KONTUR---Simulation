using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    public sealed class ProgressionSetter : MonoBehaviour, IInitializable, IDestroyable
    {
        [SerializeField] private string _key;
        
        private ProgressionService _progressionService;

        public void Set(int value)
        {
            _progressionService.SetState(_key, value);
        }

        public void OnCreate()
        {
            _progressionService = ServiceLocator.Get<ProgressionService>();
        }

        public void OnDestroyed()
        {
            _progressionService = null;
        }
    }
}