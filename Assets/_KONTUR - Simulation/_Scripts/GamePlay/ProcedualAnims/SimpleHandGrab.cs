using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public class SimpleHandGrab : MonoBehaviour
    {
        [Header("Animation Rigging")] [SerializeField]
        private Rig rigLayer; // Rig с тушкой/рукой

        [SerializeField] private Transform ikTarget; // IK Target руки
        [SerializeField] private Transform shoulder; // Кость плеча

        [Header("Side Settings")] [Tooltip("1 для правой руки, -1 для левой")] [SerializeField]
        private float sideMultiplier = 1f;

        [Header("Raycast / Direction Settings")] [SerializeField]
        private LayerMask environmentLayer;

        [SerializeField] private float rayDistance = 1.5f;
        [SerializeField] private float sphereRadius = 0.2f;
        [SerializeField] private float handOffset = 0.05f;

        [Tooltip(
            "Угол горизонтального отклонения от transform.forward в градусах (0 = прямо, 45 = диагональ, 90 = вбок)")]
        [Range(0f, 90f)]
        [SerializeField]
        private float rayAngleOffset = 30f;

        [Header("Randomization")]
        [Tooltip("Диапазон случайного смещения руки по высоте (X = мин, Y = макс в метрах)")]
        [SerializeField]
        private Vector2 verticalRandomRange = new Vector2(-0.15f, 0.25f);

        [Header("Reach & Release")] [SerializeField]
        private float maxReachDistance = 1.6f;

        [SerializeField] private float blendTime = 0.2f;

        // Внутреннее состояние
        private bool _isGrabbing = false;
        private Vector3 _grabWorldPos;
        private Quaternion _grabWorldRot;
        private Tween _weightTween;

        private void Update()
        {
            if (_isGrabbing)
            {
                // --- СОСТОЯНИЕ 1: ДЕРЖИМСЯ ЗА ТОЧКУ ---

                ikTarget.position = _grabWorldPos;
                ikTarget.rotation = _grabWorldRot;

                float currentDist = Vector3.Distance(shoulder.position, _grabWorldPos);

                Vector3 dirToPoint = (_grabWorldPos - shoulder.position).normalized;
                float dotForward = Vector3.Dot(transform.forward, dirToPoint);

                // Если вытянулся слишком далеко или точка осталась позади — отпускаем
                if (currentDist > maxReachDistance || dotForward < -0.2f)
                {
                    Release();
                }
            }
            else
            {
                // --- СОСТОЯНИЕ 2: ИЩЕМ ПОВЕРХНОСТЬ ---

                // Рассчитываем вектор с учетом угла и стороны (левая/правая)
                Vector3 castDir = GetRayDirection();

                if (Physics.SphereCast(shoulder.position, sphereRadius, castDir, out RaycastHit hit, rayDistance,
                        environmentLayer))
                {
                    // Вычисляем случайный офсет по высоте
                    float randomY = Random.Range(verticalRandomRange.x, verticalRandomRange.y);

                    // Базовая точка попадания + отступ от стены + случайная высота
                    _grabWorldPos = hit.point + (hit.normal * handOffset) + (Vector3.up * randomY);
                    _grabWorldRot = Quaternion.LookRotation(-hit.normal, transform.up);

                    Grab();
                }
            }
        }

        // Вспомогательный метод расчете направления луча
        private Vector3 GetRayDirection()
        {
            // Вращаем transform.forward вокруг оси Y на угол rayAngleOffset с учетом стороны
            float angle = rayAngleOffset * sideMultiplier;
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            return (rotation * transform.forward).normalized;
        }

        private void Grab()
        {
            _isGrabbing = true;

            _weightTween?.Kill();
            _weightTween = DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 1f, blendTime);
        }

        private void Release()
        {
            _isGrabbing = false;

            _weightTween?.Kill();
            _weightTween = DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 0f, blendTime);
        }

        // Отрисовка Гизмо для удобной настройки направления в Scene View
        private void OnDrawGizmosSelected()
        {
            if (shoulder == null) return;

            Vector3 castDir = GetRayDirection();

            Gizmos.color = _isGrabbing ? Color.green : Color.red;
            Gizmos.DrawRay(shoulder.position, castDir * rayDistance);
            Gizmos.DrawWireSphere(shoulder.position + (castDir * rayDistance), sphereRadius);

            if (_isGrabbing)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_grabWorldPos, 0.1f);
            }
        }
    }
}