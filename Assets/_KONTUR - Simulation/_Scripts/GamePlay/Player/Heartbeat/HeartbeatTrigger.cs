using System.Collections.Generic;
using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player.Heartbeat
{
    public sealed class HeartbeatTrigger : MonoBehaviour, ISpawnable, IDespawnable
    {
        public static HashSet<HeartbeatTrigger> Triggers { get; } = new();

        [SerializeField] private float _maxDistance;
        [SerializeField] private float _minDistance;

        public void OnSpawn()
        {
            Triggers.Add(this);
        }

        public void OnDespawn()
        {
            Triggers.Remove(this);
        }
        
        public float GetIntensity(Vector3 playerPosition)
        {
            var distance = Vector3.Distance(transform.position, playerPosition);

            if (distance >= _maxDistance) return 0f;
            if (distance <= _minDistance) return 1f;

            return 1f - ((distance - _minDistance) / (_maxDistance - _minDistance));
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _maxDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _minDistance);
        }
    }
}