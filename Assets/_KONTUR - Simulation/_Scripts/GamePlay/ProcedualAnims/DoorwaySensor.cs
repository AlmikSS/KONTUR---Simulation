using TriInspector;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public sealed class DoorwaySensor : MonoBehaviour, IEnvironmentSensor
    {
        [Title("SphereCast Settings")]
        [SerializeField] private LayerMask _doorFrameLayer;
        [SerializeField] private float _castRadius = 0.25f; 
        [SerializeField] private float _maxDistance = 2.0f;  
        [SerializeField] private float _handOffset = 0.02f;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Transform _directionOrigin;
        [SerializeField] private bool _invert;

        public SensorResult Evaluate()
        {
            var result = new SensorResult();
            var right = _invert ? -_directionOrigin.right : _directionOrigin.right;
            var castDirection = (_directionOrigin.forward + right * 0.4f).normalized;

            if (Physics.SphereCast(transform.position, _castRadius, castDirection, out RaycastHit hit, _maxDistance, _doorFrameLayer))
            {
                result.IsValid = true;
                var distanceFactor = 1f - (hit.distance / _maxDistance);
                result.Score = 0.6f + (distanceFactor * 0.4f);
                result.Position = hit.point + _offset + hit.normal * _handOffset;
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
            var castDirection = (_directionOrigin.forward + right * 0.4f).normalized;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, castDirection * _maxDistance);
            Gizmos.DrawWireSphere(transform.position + castDirection * _maxDistance, _castRadius);
        }
    }
}