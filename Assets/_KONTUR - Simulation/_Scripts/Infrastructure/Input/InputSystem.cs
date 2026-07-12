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

        public void Initialize()
        {
            _inputActions = new InputActions();
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
            
            if (_inputActions.Player.Interact.WasPressedThisFrame())
                _interactInput = true;
            
            if (_inputActions.Player.SecondInteraction.WasPressedThisFrame())
                _secondInteractInput = true;
            
            if (_inputActions.Player.Crouch.WasPressedThisFrame())
                _crouchInput = true;
            
            if (_inputActions.Player.Slot1.WasPressedThisFrame())
                _slot1Input = true;
            
            if (_inputActions.Player.Slot2.WasPressedThisFrame())
                _slot2Input = true;
            
            if (_inputActions.Player.Slot3.WasPressedThisFrame())
                _slot3Input = true;
            
            if (_inputActions.Player.Slot4.WasPressedThisFrame())
                _slot4Input = true;

            if (_inputActions.Player.OpenConsole.WasPressedThisFrame())
                _openConsole = true;

            if (_inputActions.Player.Jump.WasPressedThisFrame())
                _jumpInput = true;

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