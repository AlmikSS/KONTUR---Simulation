using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public class BillboardY : MonoBehaviour
    {
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void LateUpdate()
        {
            var dir = _cam.transform.position - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-dir);
        }
    }
}