using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class NpcRoute : MonoBehaviour
    {
        private Transform[] _waypoints;
        private int _currentWaypointIndex = 0;

        public bool HasRoute => _waypoints != null && _waypoints.Length > 0;

        public void Initialize(Transform[] waypoints)
        {
            _waypoints = waypoints;
        }
        
        public Vector3 GetCurrentWaypoint() 
        {
            return _waypoints[_currentWaypointIndex].position;
        }

        public void MoveToNextWaypoint()
        {
            if (!HasRoute) return;
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        }
    }
}