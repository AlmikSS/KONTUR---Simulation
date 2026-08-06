using TriInspector;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    [CreateAssetMenu(menuName = "Configs/NPC Config")]
    public sealed class NpcConfig : ScriptableObject
    {
        [SerializeField] private float _chaseSpeed;
        [SerializeField] private float _patrolSpeed;
        [SerializeField, MinMaxSlider(0, 10f)] private Vector2 _waitMinMaxTime;
        [SerializeField, MinMaxSlider(0, 10f)] private Vector2 _bewildermentMinMaxTime;
        [SerializeField] private float _trailLossTimeout;
        [SerializeField] private float _attackDistance;
        
        public float ChaseSpeed => _chaseSpeed;
        public float PatrolSpeed => _patrolSpeed;
        public float MinWaitTime => _waitMinMaxTime.x;
        public float MaxWaitTime => _waitMinMaxTime.y;
        public float MinBewildermentTime => _bewildermentMinMaxTime.x;
        public float MaxBewildermentTime => _bewildermentMinMaxTime.y;
        public float TrailLossTimeout => _trailLossTimeout;
        public float AttackDistance => _attackDistance;
    }
}