using Core.Input;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.Input
{
    public sealed class InputSystem : MonoBehaviour, ITickable, IService
    {
        private InputActions _inputActions;
        private InputSnapshot _snapshot;
        private InputContext _context;
        private bool _isConstruct;
        private bool _interactInput;
        private bool _secondInteractInput;
        private bool _crouchInput;
        private bool _openConsole;
        private bool _jumpInput;
        private bool _slot1Input;
        private bool _slot2Input;
        private bool _slot3Input;
        private bool _slot4Input;
        private int _uiOpenedCount;
        private Vector2 _currentMoveInput;
        private Vector2 _currentLookInput;
        
        public TickPhase Phase => TickPhase.InputPhase;
        public InputSnapshot Snapshot => _snapshot;
        public InputContext Context => _context;
        public Vector2 CurrentMoveInput => _currentMoveInput;
        public Vector2 CurrentLookInput => _currentLookInput;

        public void Initialize()
        {
            _inputActions = new InputActions();
            
            _inputActions.Player.Interact.performed += _ => _interactInput = true;
            _inputActions.Player.SecondInteraction.performed += _ => _secondInteractInput = true;
            _inputActions.Player.Crouch.performed += _ => _crouchInput = true;
            _inputActions.Player.OpenConsole.performed += _ => _openConsole = true;
            _inputActions.Player.Jump.performed += _ => _jumpInput = true;
            _inputActions.Player.Slot1.performed += _ => _slot1Input = true;
            _inputActions.Player.Slot2.performed += _ => _slot2Input = true;
            _inputActions.Player.Slot3.performed += _ => _slot3Input = true;
            _inputActions.Player.Slot4.performed += _ => _slot4Input = true;
            
            _inputActions.Enable();
            _isConstruct = true;
        }

        public void OpenUI()
        {
            _uiOpenedCount++;
            _context = InputContext.UI;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseUI()
        {
            _uiOpenedCount--;
            
            if (_uiOpenedCount > 0)
                return;

            _context = InputContext.GamePlay;
            _uiOpenedCount = 0;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Tick(float deltaTime)
        {
            _snapshot = new InputSnapshot(
                _context,
                _currentMoveInput,
                _currentLookInput,
                _interactInput,
                _openConsole,
                _jumpInput,
                _secondInteractInput,
                _crouchInput,
                _slot1Input,
                _slot2Input,
                _slot3Input,
                _slot4Input);
            
            _interactInput = false;
            _secondInteractInput = false;
            _crouchInput = false;
            _slot1Input = false;
            _slot2Input = false;
            _slot3Input = false;
            _slot4Input = false;
            _openConsole = false;
            _jumpInput = false;
        }

        private void Update()
        {
            if (!_isConstruct) 
                return;
            
            _currentMoveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            _currentLookInput = _inputActions.Player.Look.ReadValue<Vector2>();
        }

        private void OnDestroy()
        {
            _isConstruct = false;
            _inputActions.Disable();
        }
    }
}