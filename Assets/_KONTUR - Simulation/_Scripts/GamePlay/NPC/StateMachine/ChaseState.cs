using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class ChaseState : IState
    {
        private readonly NpcBrain _npcBrain;
        private readonly StateMachine _stateMachine;
        
        public string Name => "Chase";

        public ChaseState(NpcBrain npcBrain, StateMachine stateMachine)
        {
            _npcBrain = npcBrain;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _npcBrain.Agent.ResetPath();
            _npcBrain.Agent.speed = _npcBrain.Config.ChaseSpeed;
        }

        public void Update(float deltaTime)
        {
            if (!_npcBrain.Blackboard.IsPlayerInVision)
            {
                _stateMachine.ChangeState(_npcBrain.PatrolState);
                return;
            }
            
            _npcBrain.Agent.SetDestination(_npcBrain.Blackboard.PlayerPosition);
        }

        public void Exit() { }
    }
}