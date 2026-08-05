using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using KofeyekToolkit.Events;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class ChaseState : IState
    {
        private readonly EventBus _eventBus;
        private readonly NpcBrain _npcBrain;
        private readonly StateMachine _stateMachine;

        private float _timeSinceLastSeen;

        public string Name => "Chase";

        public ChaseState(NpcBrain npcBrain, StateMachine stateMachine)
        {
            _npcBrain = npcBrain;
            _stateMachine = stateMachine;
            _eventBus = ServiceLocator.Get<EventBus>();
        }

        public void Enter()
        {
            _npcBrain.Agent.ResetPath();
            _npcBrain.Agent.speed = _npcBrain.Config.ChaseSpeed;
            _timeSinceLastSeen = 0f;
            _eventBus.Invoke(new ChaseStartedEvent());
        }

        public void Update(float deltaTime)
        {
            if (_npcBrain.Blackboard.IsPlayerInVision && !_npcBrain.Blackboard.IsPlayerHidden)
            {
                _timeSinceLastSeen = 0f;
            }
            else
            {
                _timeSinceLastSeen += deltaTime;

                if (_timeSinceLastSeen >= _npcBrain.Config.TrailLossTimeout)
                {
                    _stateMachine.ChangeState(_npcBrain.BewildermentState);
                    return;
                }
            }

            _npcBrain.Agent.SetDestination(_npcBrain.Blackboard.PlayerPosition);
        }

        public void Exit()
        {
            _eventBus.Invoke(new ChaseEndedEvent());
        }
    }
}