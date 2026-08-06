using UnityEngine;

public class HandSurfaceAlignment : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float probeDistance = 0.4f;    // Длина щупов
    [SerializeField] private float probeStartOffset = 0.2f; // Насколько отодвинуть начало луча от ладони
    [SerializeField] private float surfaceOffset = 0.03f;   // Толщина ладони (чтобы не тонула в текстуре)

    [Header("Probe Geometry (Размеры ладони)")]
    [Tooltip("Расстояние от запястья до костяшек пальцев")]
    [SerializeField] private float forwardSpread = 0.1f;
    [Tooltip("Ширина ладони (расстояние между левым и правым щупом)")]
    [SerializeField] private float sideSpread = 0.06f;

    [Header("Smoothness")]
    [SerializeField] private float alignSpeed = 20f;

    // Включаем коррекцию только когда макро-контроллер активировал захват
    public bool IsActive { get; set; } = false;

    private Vector3[] _probeOrigins = new Vector3[3];
    private RaycastHit[] _hits = new RaycastHit[3];

    private void LateUpdate()
    {
        if (!IsActive) return;

        AlignHandToSurface();
    }

    private void AlignHandToSurface()
    {
        // 1. Рассчитываем 3 точки отправки лучей относительно текущего направления руки
        // Предполагается: transform.forward смотрит в стену, transform.up - вдоль пальцев вверх
        Vector3 rayDir = transform.forward;
        Vector3 startCenter = transform.position - rayDir * probeStartOffset;

        _probeOrigins[0] = startCenter; // Основание ладони
        _probeOrigins[1] = startCenter + transform.up * forwardSpread - transform.right * sideSpread; // Левая костяшка
        _probeOrigins[2] = startCenter + transform.up * forwardSpread + transform.right * sideSpread; // Правая костяшка

        int hitCount = 0;
        Vector3 averageNormal = Vector3.zero;
        Vector3 averagePoint = Vector3.zero;

        // 2. Пускаем 3 луча
        for (int i = 0; i < 3; i++)
        {
            if (Physics.Raycast(_probeOrigins[i], rayDir, out _hits[i], probeDistance, environmentLayer))
            {
                hitCount++;
                averageNormal += _hits[i].normal;
                averagePoint += _hits[i].point;
            }
        }

        // 3. Если зацепили поверхность — корректируем IK Target
        if (hitCount > 0)
        {
            averageNormal = (averageNormal / hitCount).normalized;
            averagePoint /= hitCount;

            // --- КОРРЕКЦИЯ ВРАЩЕНИЯ ---
            // Сохраняем текущее направление "вверх" для пальцев, но выравниваем ладонь по нормали
            Vector3 currentUp = transform.up;
            Quaternion targetRotation = Quaternion.LookRotation(-averageNormal, currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * alignSpeed);

            // --- КОРРЕКЦИЯ ПОЗИЦИИ ---
            // Проецируем текущую позицию таргета на плоскость откоса с учетом толщины ладони
            Vector3 targetPosition = averagePoint + averageNormal * surfaceOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * alignSpeed);
        }
    }

    // Визуализация треугольника лучей в Scene View
    private void OnDrawGizmosSelected()
    {
        Vector3 rayDir = transform.forward;
        Vector3 startCenter = transform.position - rayDir * probeStartOffset;

        Vector3[] debugOrigins = new Vector3[]
        {
            startCenter,
            startCenter + transform.up * forwardSpread - transform.right * sideSpread,
            startCenter + transform.up * forwardSpread + transform.right * sideSpread
        };

        Gizmos.color = IsActive ? Color.cyan : Color.gray;

        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawRay(debugOrigins[i], rayDir * probeDistance);
            Gizmos.DrawWireSphere(debugOrigins[i] + rayDir * probeDistance, 0.015f);
        }

        // Линии треугольника ладони
        Gizmos.DrawLine(debugOrigins[0], debugOrigins[1]);
        Gizmos.DrawLine(debugOrigins[1], debugOrigins[2]);
        Gizmos.DrawLine(debugOrigins[2], debugOrigins[0]);
    }
}