using UnityEngine;
using UnityEngine.AI;

namespace _KONTUR___Simulation._Scripts.GamePlay.Tests
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class TestMover : MonoBehaviour
    {
        [SerializeField] private Transform _point;
        
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.SetDestination(_point.position);
        }
    }
}