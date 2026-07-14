using KofeyekToolkit.Events;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    public sealed class ProgressionStateChangedEvent : IGameEvent
    {
        public readonly ProgressionService ProgressionService;
        public readonly string StateKey;

        public ProgressionStateChangedEvent(ProgressionService progressionService, string stateKey)
        {
            ProgressionService = progressionService;
            StateKey = stateKey;
        }
    }
}