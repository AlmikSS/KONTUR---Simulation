using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using Core.Input;
using KofeyekToolkit.DevConsole;
using TriInspector;
using UnityEngine;

namespace GamePlay.Player
{
    public class FreeCam : MonoBehaviour
    {
        [Title("Dependencies")]
        [SerializeField] private MonoBehaviour[] _componentsToDisable;
        [SerializeField] private Transform _cameraTransform;
        
        [Title("Settings")]
        [SerializeField, Slider(1f, 50f)] private float _moveSpeed = 10f;
        [SerializeField, Slider(0.01f, 1f)] private float _lookSensitivity = 0.1f;
        
        private InputSystem _inputSystem;
        private bool _isActive;
        private Transform _originalParent;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;
        
        private Vector2 _lookRotation;

        private void Start()
        {
            _inputSystem = ServiceLocator.Get<InputSystem>();
        }

        private void LateUpdate()
        {
            if (!_isActive || _inputSystem == null || _inputSystem.Snapshot.Context != InputContext.GamePlay)
                return;
                
            var deltaTime = Time.deltaTime;
            var lookInput = _inputSystem.Snapshot.LookInput;
            var moveInput = _inputSystem.Snapshot.MoveInput;
            
            _lookRotation.x -= lookInput.y * _lookSensitivity;
            _lookRotation.y += lookInput.x * _lookSensitivity;
            _lookRotation.x = Mathf.Clamp(_lookRotation.x, -90f, 90f);
            
            _cameraTransform.rotation = Quaternion.Euler(_lookRotation.x, _lookRotation.y, 0f);
            
            var moveDir = _cameraTransform.forward * moveInput.y + _cameraTransform.right * moveInput.x;
            _cameraTransform.position += moveDir * (_moveSpeed * deltaTime);
        }

        [Command("freecam", "Enable/disable free camera mode")]
        private void SetFreeCam(bool enable)
        {
            if (_isActive == enable)
                return;
                
            _isActive = enable;
            
            if (_isActive)
            {
                if (_cameraTransform != null)
                {
                    _originalParent = _cameraTransform.parent;
                    _originalLocalPosition = _cameraTransform.localPosition;
                    _originalLocalRotation = _cameraTransform.localRotation;
                    
                    _cameraTransform.SetParent(null);
                    
                    var euler = _cameraTransform.eulerAngles;
                    _lookRotation = new Vector2(euler.x, euler.y);
                    if (_lookRotation.x > 180f) _lookRotation.x -= 360f;
                }
                
                SetComponentsEnabled(false);
                Debug.Log("FreeCam Enabled");
            }
            else
            {
                if (_cameraTransform != null)
                {
                    _cameraTransform.SetParent(_originalParent);
                    _cameraTransform.localPosition = _originalLocalPosition;
                    _cameraTransform.localRotation = _originalLocalRotation;
                }
                
                SetComponentsEnabled(true);
                Debug.Log("FreeCam Disabled");
            }
        }

        private void SetComponentsEnabled(bool isEnabled)
        {
            if (_componentsToDisable == null)
                return;

            foreach (var comp in _componentsToDisable)
            {
                if (comp != null)
                    comp.enabled = isEnabled;
            }
        }

        [Command("set_freecam_speed", "Change freecam speed")]
        private void SetSpeed(float speed)
        {
            _moveSpeed = speed;
            Debug.Log("FreeCam speed changed: " + _moveSpeed);
        }

        [Command("set_freecam_sens", "Change freecam sensitivity")]
        private void SetSensitivity(float sensitivity)
        {
            _lookSensitivity = sensitivity;
            Debug.Log("FreeCam sensitivity changed: " + _lookSensitivity);
        }
    }
}
