using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using TMPro;
using UnityEngine;

namespace GamePlay.Player.UI
{
    public class InteractionRenderer : MonoBehaviour, ISpawnable, IDespawnable, ITickable
    {
        [SerializeField] private RectTransform _cursor;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private RectTransform _canvasRect;

        [SerializeField] private float _cursorSmoothSpeed = 10f;
        [SerializeField] private float _textSmoothSpeed = 5f;
        [SerializeField] private Vector2 _textOffset = new(0f, -30f);

        private EventBus _eventBus;
        private Camera _camera;
        private Transform _focusPoint;

        private bool _hasTarget;

        private Vector2 _currentCursorPos;
        private Vector2 _currentTextPos;
        private Vector2 _defaultCursorPos;

        public TickPhase Phase => TickPhase.PresentationPhase;

        public void OnCreate()
        {
            // Центр экрана
            _defaultCursorPos = Vector2.zero;

            _currentCursorPos = _defaultCursorPos;
            _currentTextPos = _defaultCursorPos + _textOffset;

            _cursor.anchoredPosition = _defaultCursorPos;

            if (_hintText != null)
                _hintText.rectTransform.anchoredPosition = _currentTextPos;
        }

        public void OnSpawn()
        {
            _camera = PlayerContext.Camera;

            _eventBus = ServiceLocator.Get<EventBus>();

            _eventBus.Register<InteractionShownEvent>(OnShown);
            _eventBus.Register<InteractionHiddenEvent>(OnHidden);

            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            _eventBus.Unregister<InteractionShownEvent>(OnShown);
            _eventBus.Unregister<InteractionHiddenEvent>(OnHidden);

            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unregister<InteractionShownEvent>(OnShown);
                _eventBus.Unregister<InteractionHiddenEvent>(OnHidden);
            }

            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (_camera == null)
            {
                _camera = PlayerContext.Camera;

                if (_camera == null)
                    return;
            }

            Vector2 targetPos;

            if (_hasTarget && _focusPoint != null)
            {
                Vector3 viewportPoint =
                    _camera.WorldToViewportPoint(_focusPoint.position);

                float x = (viewportPoint.x - 0.5f) * _canvasRect.rect.width;
                float y = (viewportPoint.y - 0.5f) * _canvasRect.rect.height;

                targetPos = new Vector2(x, y);
            }
            else
            {
                targetPos = _defaultCursorPos;
            }

            _currentCursorPos = Vector2.Lerp(
                _currentCursorPos,
                targetPos,
                _cursorSmoothSpeed * deltaTime
            );

            _cursor.anchoredPosition = _currentCursorPos;

            if (_hintText != null)
            {
                Vector2 textTarget = targetPos + _textOffset;

                _currentTextPos = Vector2.Lerp(
                    _currentTextPos,
                    textTarget,
                    _textSmoothSpeed * deltaTime
                );

                _hintText.rectTransform.anchoredPosition = _currentTextPos;
                _hintText.gameObject.SetActive(_hasTarget);
            }
        }

        private void OnShown(InteractionShownEvent e)
        {
            _focusPoint = e.FocusPoint;

            if (_hintText != null)
            {
                _hintText.text = string.IsNullOrEmpty(e.Interactable.ActionText) 
                    ? "Interact" 
                    : e.Interactable.ActionText;
            }

            _hasTarget = true;
        }

        private void OnHidden(InteractionHiddenEvent e)
        {
            _focusPoint = null;
            _hasTarget = false;
        }
    }
}