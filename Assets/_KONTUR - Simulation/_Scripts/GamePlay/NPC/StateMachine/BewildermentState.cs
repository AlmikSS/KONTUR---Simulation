using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class BewildermentState : IState
    {
        private readonly NpcBrain _npcBrain;
        private readonly StateMachine _stateMachine;
        private float _waitTimer;
        private bool _isWaiting;
        
        public string Name => "BewildermentStateBewildermentState";

        public BewildermentState(NpcBrain npcBrain, StateMachine stateMachine)
        {
            _npcBrain = npcBrain;
            _stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            _npcBrain.Agent.speed = _npcBrain.Config.PatrolSpeed;
        }

        public void Update(float deltaTime)
        {
            if (_npcBrain.Blackboard.IsPlayerInVision)
            {
                _stateMachine.ChangeState(_npcBrain.ChaseState);
                return;
            }
            
            switch (_isWaiting)
            {
                case false when !_npcBrain.Agent.hasPath || _npcBrain.Agent.remainingDistance < 0.5f:
                    StartWaiting();
                    return;
                case true:
                {
                    _waitTimer -= deltaTime;
                    if (_waitTimer <= 0)
                    {
                        _stateMachine.ChangeState(_npcBrain.PatrolState);
                    }

                    break;
                }
            }
        }
        
        private void StartWaiting()
        {
            _isWaiting = true;
            _waitTimer = Random.Range(_npcBrain.Config.MinBewildermentTime, _npcBrain.Config.MaxBewildermentTime);
            _npcBrain.Agent.ResetPath();
        }

        public void Exit()
        {
            _isWaiting = false;
            _waitTimer = 0;
        }
    }
}