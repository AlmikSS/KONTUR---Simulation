using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.TickSystem;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Player
{
    public class PlayerInteraction : MonoBehaviour, ITickable
    {
        [SerializeField] private float _interactionDistance;
        [SerializeField] private Transform _interactionOrigin;
        [SerializeField] private Image _hintImage;
        [SerializeField] private bool _drawGizmos;
        [SerializeField, ShowIf(nameof(_drawGizmos))] private Color _gizmosColor;
        
        private InputSystem _inputSystem;
        
        public TickPhase Phase => TickPhase.SimulationPhase;

        private void Start()
        {
            _inputSystem = ServiceLocator.Get<InputSystem>();
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }
        
        public void Tick(float deltaTime)
        {
            if (!Physics.Raycast(_interactionOrigin.position, _interactionOrigin.forward, out var hit, _interactionDistance) 
                || !hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                ResetHint();
                return;
            }

            if (interactable.HasHint)
            {
                _hintImage.color = new Color(1, 1, 1, 1);
                _hintImage.sprite = interactable.HintSprite;
            }
            else
                ResetHint();
            
            if (_inputSystem.Snapshot.InteractInput)
                interactable.Interact(gameObject);
        }

        private void ResetHint()
        {
            _hintImage.color = new Color(1, 1, 1, 0);
            _hintImage.sprite = null;
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