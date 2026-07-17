using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public sealed class HandController : MonoBehaviour
    {
        [SerializeField] private float _evoluteTime;
        [SerializeField] private string _triggerTag = "Door";
        [SerializeField] private LayerMask _mask;

        private Vector3 _targetPosition;
        private bool _isMove;
        
        private void Update()
        {
            if (!_isMove)
                return;
            
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _evoluteTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                _isMove = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.tag);
            
            if (!other.CompareTag(_triggerTag))
                return;
            
            Debug.Log(1);
            
            var dirToDoor = (other.transform.position - transform.position).normalized;
            dirToDoor.z = -dirToDoor.z;
            // Debug.DrawRay(transform.position, dirToDoor, Color.red, 100);
            // Debug.Break();
            if (Physics.Raycast(transform.position, dirToDoor, out var hit, Mathf.Infinity, _mask))
            {
                Debug.Log(2);
                _targetPosition = hit.point;
                _isMove = true;
            }
        }
    }
}