using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public sealed class ProceduralHandController : MonoBehaviour
    {
        [Title("Rig")] [SerializeField] private Rig _rigLayer;
        [SerializeField] private Transform _ikTarget;

        [Title("Movement settings")]
        [SerializeField] private float _targetFollowSpeed;
        [SerializeField] private float _weightBlendTime = 0.25f;
        [SerializeField] private float _hysteresisBonus = 0.3f;

        private List<IEnvironmentSensor> _sensors = new();
        private IEnvironmentSensor _activeSensor;

        private Tween _weightTween;
        private bool _isAttached;

        private void Awake()
        {
            _sensors = GetComponents<IEnvironmentSensor>().ToList();
        }

        private void Update()
        {
            IEnvironmentSensor bestSensor = null;
            SensorResult bestResult = default;
            var highestScore = -1f;

            foreach (var sensor in _sensors)
            {
                var result = sensor.Evaluate();
                if (!result.IsValid) continue;

                var currentBonus = (sensor == _activeSensor) ? _hysteresisBonus : 0f;
                var finalScore = result.Score + currentBonus;

                if (finalScore > highestScore)
                {
                    highestScore = finalScore;
                    bestResult = result;
                    bestSensor = sensor;
                }
            }

            if (bestSensor != null)
            {
                _activeSensor = bestSensor;

                _ikTarget.position = Vector3.Lerp(_ikTarget.position, bestResult.Position, Time.deltaTime * _targetFollowSpeed);
                _ikTarget.rotation = Quaternion.Slerp(_ikTarget.rotation, bestResult.Rotation, Time.deltaTime * _targetFollowSpeed);

                if (!_isAttached)
                {
                    AttachHand();
                }
            }
            else
            {
                if (_isAttached)
                {
                    DetachHand();
                }
            }
        }

        private void AttachHand()
        {
            _isAttached = true;

            _weightTween?.Kill();
            _weightTween = DOTween.To(() => _rigLayer.weight, x => _rigLayer.weight = x, 1f, _weightBlendTime).SetEase(Ease.OutQuad);
        }

        private void DetachHand()
        {
            _isAttached = false;
            _activeSensor = null;

            _weightTween?.Kill();
            _weightTween = DOTween.To(() => _rigLayer.weight, x => _rigLayer.weight = x, 0f, _weightBlendTime).SetEase(Ease.InQuad);
        }

        public void ForceDetach()
        {
            if (!_isAttached) return;

            DetachHand();
        }
    }
}