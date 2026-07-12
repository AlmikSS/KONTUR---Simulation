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
        
        public float ChaseSpeed => _chaseSpeed;
        public float PatrolSpeed => _patrolSpeed;
        public float MinWaitTime => _waitMinMaxTime.x;
        public float MaxWaitTime => _waitMinMaxTime.y;
    }
}