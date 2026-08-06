using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public class ProceduralGaitHand : MonoBehaviour
    {
        [Title("Rigging")]
        [SerializeField] private Rig rigLayer;
        [SerializeField] private Transform ikTarget;
        [SerializeField] private Transform shoulderTransform; // Кость плеча для отсчета дистанции

        [Title("Reach & Pinning Settings")]
        [SerializeField] private float maxReachRadius = 1.2f; // Максимальный радиус вытянутой руки
        [SerializeField] private float minForwardAngle = -0.3f; // Насколько сильно рука может уйти НАЗАД (-1 = строго сзади)
        [SerializeField] private float handOffset = 0.05f;

        [Title("Animation Tweens")]
        [SerializeField] private float attachDuration = 0.2f;
        [SerializeField] private float detachDuration = 0.25f;

        private List<IEnvironmentSensor> _sensors;
        private bool _isGrabbing = false;

        // Зафиксированные мировые координаты и поворот
        private Vector3 _lockedWorldPos;
        private Quaternion _lockedWorldRot;

        private Tween _weightTween;
        private Tween _moveTween;

        private void Awake()
        {
            _sensors = GetComponentsInChildren<IEnvironmentSensor>().ToList();
        }

        private void Update()
        {
            if (_isGrabbing)
            {
                // === СОСТОЯНИЕ 1: РУКА ДЕРЖИТ УСТУП ===

                // 1. Удерживаем IK-таргет строго в мировых координатах
                ikTarget.position = _lockedWorldPos;
                ikTarget.rotation = _lockedWorldRot;

                // 2. Проверяем, не пора ли отпустить
                if (ShouldRelease())
                {
                    ReleaseGrab();
                }
            }
            else
            {
                // === СОСТОЯНИЕ 2: ПОИСК НОВОГО УСТУПА ===

                SensorResult bestResult = FindBestTarget();

                if (bestResult.IsValid)
                {
                    // Нашли за что зацепиться — захватываем!
                    GrabPoint(bestResult.Position, bestResult.Rotation);
                }
            }
        }

        private SensorResult FindBestTarget()
        {
            SensorResult best = default;
            float highestScore = -1f;

            foreach (var sensor in _sensors)
            {
                SensorResult res = sensor.Evaluate();
                if (res.IsValid && res.Score > highestScore)
                {
                    highestScore = res.Score;
                    best = res;
                }
            }

            return best;
        }

        private void GrabPoint(Vector3 worldPos, Quaternion worldRot)
        {
            _isGrabbing = true;
            _lockedWorldPos = worldPos;
            _lockedWorldRot = worldRot;

            // Отменяем старые твины
            _moveTween?.Kill();
            _weightTween?.Kill();

            // 1. Анимируем бросок руки к зафиксированной точке (слегка дугой через Ease.OutBack или параболу)
            _moveTween = ikTarget.DOMove(_lockedWorldPos, attachDuration).SetEase(Ease.OutQuad);
            ikTarget.DORotateQuaternion(_lockedWorldRot, attachDuration);

            // 2. Включаем вес рига
            _weightTween = DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 1f, attachDuration);
        }

        private bool ShouldRelease()
        {
            // 1. Проверка по расстоянию от плеча
            float distance = Vector3.Distance(shoulderTransform.position, _lockedWorldPos);
            if (distance > maxReachRadius) return true;

            // 2. Проверка по углу (если точка осталась слишком далеко позади персонажа)
            Vector3 directionToPoint = (_lockedWorldPos - shoulderTransform.position).normalized;
            float forwardDot = Vector3.Dot(transform.forward, directionToPoint);

            if (forwardDot < minForwardAngle) return true;

            return false;
        }

        private void ReleaseGrab()
        {
            _isGrabbing = false;

            _weightTween?.Kill();
            _moveTween?.Kill();

            // Плавно выключаем IK, рука плавно возвращается в базовую анимацию ходьбы
            _weightTween = DOTween.To(() => rigLayer.weight, x => rigLayer.weight = x, 0f, detachDuration)
                .SetEase(Ease.InOutQuad);
        }

        private void OnDrawGizmosSelected()
        {
            if (shoulderTransform != null)
            {
                // Визуализация максимальной зоны досягаемости руки
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(shoulderTransform.position, maxReachRadius);
            }
        }
    }
}