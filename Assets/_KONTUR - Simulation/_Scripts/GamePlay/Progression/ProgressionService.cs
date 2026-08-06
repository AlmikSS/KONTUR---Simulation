using System.Collections.Generic;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.Events;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    public sealed class ProgressionService : IService
    {
        private readonly EventBus _eventBus;
        private readonly Dictionary<string, int> _states = new();

        public ProgressionService()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
        }
        
        public ProgressionService(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [Command("set_progression", "sets the progression key value")]
        public void SetState(string key, int value)
        {
            _states[key] = value;
            _eventBus.Invoke(new ProgressionStateChangedEvent(this, key));
        }
        
        public int GetState(string key)
        {
            return _states.GetValueOrDefault(key, 0);
        }
    }
}