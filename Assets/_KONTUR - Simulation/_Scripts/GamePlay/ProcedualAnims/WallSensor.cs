using TriInspector;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public sealed class WallSensor : MonoBehaviour, IEnvironmentSensor
    {
        [Title("Raycast Settings")]
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private float _maxDistance = 1.5f;
        [SerializeField] private float _handOffset = 0.05f;
        [SerializeField] private Transform _directionOrigin;
        [SerializeField] private bool _invert;

        public SensorResult Evaluate()
        {
            var result = new SensorResult();
            var right = _invert ? -_directionOrigin.right : _directionOrigin.right;
            var rayDirection = (right + _directionOrigin.forward * 0.3f).normalized;

            if (Physics.Raycast(transform.position, rayDirection, out var hit, _maxDistance, _wallLayer))
            {
                result.IsValid = true;
            
                var distanceFactor = 1f - (hit.distance / _maxDistance);
                result.Score = distanceFactor * 0.5f; 

                result.Position = hit.point + hit.normal * _handOffset;
        
                result.Rotation = Quaternion.LookRotation(-hit.normal, transform.up);
            }
            else
            {
                result.IsValid = false;
            }

            return result;
        }
        
        private void OnDrawGizmosSelected()
        {
            var right = _invert ? -_directionOrigin.right : _directionOrigin.right;
            var rayDirection = (right + _directionOrigin.forward * 0.3f).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, rayDirection * _maxDistance);
        }
    }
}