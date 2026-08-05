using _KONTUR___Simulation._Scripts;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Player.UI
{
    public class StaminaRenderer : MonoBehaviour, ISpawnable, IDespawnable
    {
        [SerializeField] private Image _vignette;
        [SerializeField] private float _vignetteThreshold;
        [SerializeField] private RectTransform _staminaScale;

        private EventBus _eventBus;

        public void OnSpawn()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<OnStaminaChangedEvent>(OnStaminaChange);

            // Reset current state
            ApplyStamina(1, 1);
        }

        public void OnDespawn()
        {
            _eventBus.Unregister<OnStaminaChangedEvent>(OnStaminaChange);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
                _eventBus.Unregister<OnStaminaChangedEvent>(OnStaminaChange);
        }

        private void OnStaminaChange(OnStaminaChangedEvent e)
        {
            ApplyStamina(e.CurrentValue, e.Component.MaxStamina);
        }

        private void ApplyStamina(float currentValue, float maxValue)
        {
            float normalized = currentValue / maxValue;
            _staminaScale.localScale = new Vector3(normalized, 1f, 1f);

            if (normalized <= _vignetteThreshold)
            {
                float alpha = 1f - (normalized / _vignetteThreshold);
                Color color = _vignette.color;
                color.a = alpha;
                _vignette.color = color;
            }
            else
            {
                Color color = _vignette.color;
                color.a = 0f;
                _vignette.color = color;
            }
        }
    }
}