using _KONTUR___Simulation._Scripts.GamePlay.NPC.Sensors;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using UnityEngine.AI;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain
{
    [SelectionBase]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NpcBrain : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public NpcRoute Route { get; private set; }
        [field: SerializeField] public NpcConfig Config { get; private set; }
        [SerializeField] private float _updateInterval = 0.2f;
        [SerializeField] private NpcVision _vision;
        
        private StateMachine _stateMachine;
        private float _updateTimer;
        private float _lastUpdateTime;
        private bool _isInit;
        
        public WaypointsPatrolState PatrolState { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public BewildermentState BewildermentState { get; private set; }
        
        public NpcBlackboard Blackboard { get; private set; }
        public StateMachine StateMachine => _stateMachine;
        public TickPhase Phase => TickPhase.SimulationPhase;
        public bool IsInit => _isInit;
        public float UpdateTimer => _updateTimer;
        public float LastUpdateTime => _lastUpdateTime;

        public void OnSpawn()
        {
            Blackboard = new NpcBlackboard();
            _updateTimer = Random.Range(0f, _updateTimer);
            _stateMachine = new StateMachine();
            
            PatrolState = new WaypointsPatrolState(this, _stateMachine);
            ChaseState = new ChaseState(this, _stateMachine);
            BewildermentState = new BewildermentState(this, _stateMachine);
            
            _vision.Initialize(Blackboard, PlayerContext.Transform);
            _stateMachine.ChangeState(PatrolState);
            _isInit = true;
            
            ServiceLocator.Get<TickSystem>().Register(this);
        }
        
        public void Tick(float deltaTime)
        {
            if (!_isInit)
                return;
            
            _updateTimer -= deltaTime;
            if (_updateTimer <= 0)
            {
                var delta = Time.time - _lastUpdateTime;
                _stateMachine.Update(delta);
                _updateTimer = _updateInterval;
                _lastUpdateTime = Time.time;
            }
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            Blackboard = null;
            Agent = null;
            _stateMachine = null;
            _updateTimer = 0f;
            _isInit = false;
        }
    }
}