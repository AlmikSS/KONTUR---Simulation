using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class WaypointsPatrolState : IState
    {
        private readonly NpcBrain _npcBrain;
        private readonly StateMachine _stateMachine;
        private float _waitTimer;
        private bool _isWaiting;
        
        public string Name => "Waypoints Patrol";
        
        public WaypointsPatrolState(NpcBrain npcBrain, StateMachine stateMachine)
        {
            _npcBrain = npcBrain;
            _stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            _npcBrain.Agent.ResetPath();
            _npcBrain.Agent.speed = _npcBrain.Config.PatrolSpeed;
            _isWaiting = false;
            MoveToCurrentWaypoint();
        }

        public void Update(float deltaTime)
        {
            if (_npcBrain.Blackboard.IsPlayerInVision)
            {
                _stateMachine.ChangeState(_npcBrain.ChaseState);
                return;
            }

            if (_isWaiting)
            {
                _waitTimer -= deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaiting = false;
                    _npcBrain.Route.MoveToNextWaypoint();
                    MoveToCurrentWaypoint();
                }
            }
            else
            {
                if (!_npcBrain.Agent.hasPath || _npcBrain.Agent.remainingDistance < 0.5f)
                {
                    StartWaiting();
                }
            }
        }

        public void Exit()
        {
            _npcBrain.Agent.ResetPath();
        }
        
        private void StartWaiting()
        {
            _isWaiting = true;
            _waitTimer = Random.Range(_npcBrain.Config.MinWaitTime, _npcBrain.Config.MaxWaitTime);
            _npcBrain.Agent.ResetPath();
        }
        
        private void MoveToCurrentWaypoint()
        {
            if (_npcBrain.Route.HasRoute)
            {
                _npcBrain.Agent.SetDestination(_npcBrain.Route.GetCurrentWaypoint());
            }
        }
    }
}