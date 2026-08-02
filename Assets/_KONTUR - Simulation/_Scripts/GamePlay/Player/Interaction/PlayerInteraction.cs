using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.GamePlay.Interactors;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using _KONTUR___Simulation._Scripts.Input;

using KofeyekToolkit.DevConsole;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Player
{
    public class PlayerInteraction : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private float _interactionDistance;
        [SerializeField] private Transform _interactionOrigin;
        [SerializeField] private Image _hintImage;
        [SerializeField] private bool _drawGizmos;
        [SerializeField, ShowIf(nameof(_drawGizmos))] private Color _gizmosColor;
        
        private EventBus _eventBus;
        private InputSystem _inputSystem;
        private IInteractable _lastInteractable;
        private string _lastShownActionText;

        public TickPhase Phase => TickPhase.SimulationPhase;

        // lifecycle

        public void OnSpawn()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _inputSystem = ServiceLocator.Get<InputSystem>();
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            _eventBus = null;
            _inputSystem = null;
        }

        // methods
        
        public void Tick(float deltaTime)
        {
            InteractorBase bestInteractable = null;
            float bestDistance = float.MaxValue;
            
            foreach (var interactable in InteractorBase.Registry)
            {
                if (!IsInteractableInRange(interactable)) continue;
                if (!IsInteractableInView(interactable)) continue;
                if (!IsInteractableInLineOfSight(interactable)) continue;

                var viewportPoint = PlayerContext.Camera.WorldToViewportPoint(interactable.FocusPoint.position);
                float dx = viewportPoint.x - 0.5f;
                float dy = viewportPoint.y - 0.5f;
                float distToCenter = dx * dx + dy * dy;

                if (distToCenter < bestDistance)
                {
                    bestDistance = distToCenter;
                    bestInteractable = interactable;
                }
            }

            if (bestInteractable == null)
            {
                if (_lastInteractable != null)
                {
                    _eventBus.Invoke(new InteractionHiddenEvent());
                    _lastInteractable = null;
                    _lastShownActionText = null;
                }
                return;
            }

            if (_lastInteractable != bestInteractable || _lastShownActionText != bestInteractable.ActionText)
            {
                if (_lastInteractable != null)
                    _eventBus.Invoke(new InteractionHiddenEvent());

                _lastInteractable = bestInteractable;
                _lastShownActionText = bestInteractable.ActionText;
                _eventBus.Invoke(new InteractionShownEvent(bestInteractable));
            }

            if (_inputSystem.Snapshot.InteractInput)
                bestInteractable.Interact(gameObject);

            if (_inputSystem.Snapshot.SecondInteractInput)
                bestInteractable.SecondaryInteract(gameObject);
        }

        // returns true if interactable stays withing _interactionDistance
        private bool IsInteractableInRange(InteractorBase interactable)
        {
            var magnitude = interactable.FocusPoint.position - _interactionOrigin.position;
            var distanceSqr = _interactionDistance * _interactionDistance;

            if (magnitude.sqrMagnitude > distanceSqr)
                return false;

            return true;
        }

        // returns true if interactable is located on screen view
        private bool IsInteractableInView(InteractorBase interactable)
        {
            var viewportPoint = PlayerContext.Camera.WorldToViewportPoint(interactable.FocusPoint.position);

            if (viewportPoint.z < 0)
                return false;

            if (viewportPoint.x < 0 || viewportPoint.x > 1)
                return false;

            if (viewportPoint.y < 0 || viewportPoint.y > 1)
                return false;

            return true;
        }

        // returns true if there's no obstacles between player viewpoint and interactable
        private bool IsInteractableInLineOfSight(InteractorBase interactable)
        {
            var direction = interactable.FocusPoint.position - _interactionOrigin.position;
            var distance = direction.magnitude;

            if (Physics.Raycast(_interactionOrigin.position, direction.normalized, out var hit, distance))
            {
                // if hit is target interactable
                return hit.collider.gameObject == interactable.gameObject;
            }

            // ray didn't hit anything, all is ok
            return true;
        }

        private void ResetHint()
        {
            // _hintImage.color = new Color(1, 1, 1, 0);
            // _hintImage.sprite = null;
        }
        
        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;

            Gizmos.color = _gizmosColor;
            Gizmos.DrawLine(_interactionOrigin.position, _interactionOrigin.position + _interactionOrigin.forward * _interactionDistance);
        }

        [Command("set_interaction_distance", "Changed player interaction distance")]
        private void ChangeInteractionDistance(float distance)
        {
            _interactionDistance = distance;
            Debug.Log("Interaction distance set to: " + _interactionDistance);
        }
    }
}