using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public class SimpleHandGrab : MonoBehaviour
    {
        [Header("Animation Rigging")] [SerializeField]
        private Rig rigLayer; // Rig с рукой

        [SerializeField] private Transform ikTarget; // IK Target руки
        [SerializeField] private Transform shoulder; // Кость плеча

        [Header("Side Settings")] [Tooltip("1 для правой руки, -1 для левой")] [SerializeField]
        private float sideMultiplier = 1f;

        [Header("Raycast / Direction Settings")] [SerializeField]
        private LayerMask environmentLayer; // Слои стен/косяков

        [SerializeField] private float rayDistance = 1.5f;
        [SerializeField] private float sphereRadius = 0.2f; // Толщина луча

        [Tooltip("КРИТИЧНО для избежания проникновения в текстуру. Установи толщину ладони монстра.")] [SerializeField]
        private float handOffset = 0.15f; // Увеличен дефолт

        [Tooltip(
            "Угол горизонтального отклонения от transform.forward в градусах (0 = прямо, 45 = диагональ, 90 = вбок)")]
        [Range(0f, 90f)]
        [SerializeField]
        private float rayAngleOffset = 30f;

        [Header("Randomization")]
        [Tooltip("Диапазон случайного смещения руки по высоте (X = мин, Y = макс в метрах)")]
        [SerializeField]
        private Vector2 verticalRandomRange = new Vector2(-0.15f, 0.25f);

        [Header("Reach & Release (Скорости)")]
        [SerializeField] private float maxReachDistance = 1.6f; 
    
        [Tooltip("Насколько далеко позади может остаться рука (-1 = строго сзади, 0 = сбоку, 1 = спереди)")]
        [Range(-1f, 0f)]
        [SerializeField] private float releaseAngleLimit = -0.7f; // Вынесли в инспектор, по умолчанию разрешаем руке уйти далеко назад

        [SerializeField] private float blendInDuration = 0.2f; 
        [SerializeField] private float blendOutDuration = 0.25f;

        [Header("Movement Arc (Дуга)")] [SerializeField]
        private float arcHeight = 0.3f; // Высота подъема дуги

        [SerializeField] private float arcOutwardBias = 0.2f; // Насколько дуга отклоняется ОТ стены
        
        [SerializeField] private HandSurfaceAlignment surfaceAlignment;

        [Header("Events (События для анимации пальцев)")]
        [Tooltip("Вызывается, когда рука начинает бросок к уступу. Здесь включай анимацию сжатия кулака.")]
        public UnityEvent OnGrabStarted;

        [Tooltip("Вызывается, когда рука отпускает уступ. Здесь разжимай кулак.")]
        public UnityEvent OnGrabReleased;

        // Внутреннее состояние
        private bool _isGrabbing = false;
        private Vector3 _grabWorldPos;
        private Quaternion _grabWorldRot;

        private Tween _weightTween;
        private Sequence _grabSequence; // Container for movement and rotation
        private bool _isTweening = false; // Флаг, что рука сейчас в перелете

        private void Update()
        {
            if (_isGrabbing)
            {
                // Жестко фиксируем позицию, только если рука уже прилетела к стене
                if (!_isTweening)
                {
                    ikTarget.position = _grabWorldPos;
                    ikTarget.rotation = _grabWorldRot;

                    // Считаем дистанцию и угол
                    float currentDist = Vector3.Distance(shoulder.position, _grabWorldPos);
                    Vector3 dirToPoint = (_grabWorldPos - shoulder.position).normalized;
                    float dotForward = Vector3.Dot(transform.forward, dirToPoint);

                    // Отпускаем, только если перетянулись по дистанции ИЛИ рука осталась слишком далеко позади спины
                    if (currentDist > maxReachDistance || dotForward < releaseAngleLimit)
                    {
                        Release();
                    }
                }
                else 
                {
                    // Фейлсейф: если монстр сделал резкий рывок/телепорт во время анимации броска
                    if (Vector3.Distance(shoulder.position, _grabWorldPos) > maxReachDistance * 1.5f)
                    {
                        Release();
                    }
                }
            }
            else
            {
                // --- СОСТОЯНИЕ 2: ИЩЕМ ПОВЕРХНОСТЬ ---
                Vector3 castDir = GetRayDirection();

                if (Physics.SphereCast(shoulder.position, sphereRadius, castDir, out RaycastHit hit, rayDistance, environmentLayer))
                {
                    float randomY = Random.Range(verticalRandomRange.x, verticalRandomRange.y);
                    _grabWorldPos = hit.point + (hit.normal * handOffset) + (Vector3.up * randomY);
                    _grabWorldRot = Quaternion.LookRotation(-hit.normal, transform.up);

                    Grab(hit.normal);
                }
            }
        }

        private Vector3 GetRayDirection()
        {
            float angle = rayAngleOffset * sideMultiplier;
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            return (rotation * transform.forward).normalized;
        }

        private void Grab(Vector3 surfaceNormal)
        {
            if (_isTweening) return; // Не прерываем текущий бросок

            _isGrabbing = true;
            _isTweening = true;

            // Генерируем дугу движения (перелет от текущей позы к зафиксированной точке)
            Vector3 startPos = ikTarget.position;
            Vector3 endPos = _grabWorldPos;

            // Промежуточная точка дуги (поднимаем вверх + сдвигаем от стены по нормали)
            Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f)
                               + Vector3.up * arcHeight
                               + surfaceNormal * arcOutwardBias;

            // Массив точек для параболы ДОТвина
            Vector3[] path = new Vector3[] { startPos, midPoint, endPos };

            // Управление твинами через Sequence (цепочка действий)
            _grabSequence?.Kill();
            _grabSequence = DOTween.Sequence();

            // 1. Анимируем бросок руки по дуге (CatmullRom парабола)
            _grabSequence.Join(ikTarget.DOPath(path, blendInDuration, PathType.CatmullRom).SetEase(Ease.OutQuad));

            // 2. Анимируем вращение кисти к поверхности
            _grabSequence.Join(ikTarget.DORotateQuaternion(_grabWorldRot, blendInDuration).SetEase(Ease.OutQuad));

            // 3. Плавно включаем вес рига
            _grabSequence.Join(DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 1f, blendInDuration)
                .SetEase(Ease.OutQuad));

            // Событие завершения твина (переход в режим жесткого Lock-а)
            _grabSequence.OnComplete(() => _isTweening = false);

            // Триггерим события для внешних систем (анимация пальцев, звук)
            OnGrabStarted?.Invoke();
            
            if (surfaceAlignment != null) surfaceAlignment.IsActive = true;
        }

        private void Release()
        {
            if (_isTweening) _grabSequence?.Kill(); // Прерываем бросок при резком отрыве
            _isGrabbing = false;
            _isTweening = false; // Убедимся, что флаг сброшен

            // Плавно выключаем вес рига (рука вернется в обычную анимацию ходьбы/idle)
            _weightTween?.Kill();
            _weightTween = DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 0f, blendOutDuration)
                .SetEase(Ease.InOutQuad);

            if (surfaceAlignment != null) surfaceAlignment.IsActive = false;
            OnGrabReleased?.Invoke();
        }

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