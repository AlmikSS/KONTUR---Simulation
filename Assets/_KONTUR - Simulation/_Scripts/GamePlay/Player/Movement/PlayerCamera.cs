using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using Core.Input;
using KofeyekToolkit.DevConsole;
using TriInspector;
using UnityEngine;

namespace GamePlay.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Title("Dependencies")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private Transform _lookRoot;
        [SerializeField] private Transform _effectsRoot;
        
        [Title("Base options")]
        [SerializeField, Slider(0f, 100f)] private float _sensitivity;
        [SerializeField, Slider(0, 90f)] private float _xRotationClamp;
        [SerializeField, Slider(0, 1f)] private float _movementVelocityStopThreshold;
        
        [Title("Camera bob options")]
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeX;
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeY;
        [SerializeField, Slider(0, 5)] private float _bobFrequency;
        
        [Title("Movement tilt options")]
        [SerializeField, Slider(0, 15)] private float _movementTiltAmount;
        [SerializeField, Slider(0, 15)] private float _movementTiltSmoothness;
        [SerializeField, Slider(0, 30)] private float _movementTiltClamp;
        
        private InputSystem _inputSystem;
        private Vector3 _lookRotation;
        private Vector3 _movementTiltRotation;
        private Vector3 _bobPosition;
        private float _bobCycle;
        private float _currentMovementTilt;

        public Vector3 LookRotation => _lookRotation;

        private void Start()
        {
            _inputSystem = ServiceLocator.Get<InputSystem>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void LateUpdate()
        {
            if (_inputSystem == null || _inputSystem.Snapshot.Context != InputContext.GamePlay || !enabled)
                return;
            
            var deltaTime = Time.deltaTime;
            var lookInput = _inputSystem.Snapshot.LookInput;
            
            CalculateBaseMouseLook(lookInput);
            CalculateCameraBob(deltaTime);
            CalculateMovementTilt(deltaTime);
            
            var effectsRotation = _movementTiltRotation;
            var effectsPosition = _bobPosition;
            
            _orientationTransform.rotation = Quaternion.Euler(0f, _lookRotation.y, 0f);
            _lookRoot.localRotation = Quaternion.Euler(_lookRotation);
            _effectsRoot.localRotation = Quaternion.Euler(effectsRotation);
            _effectsRoot.localPosition = effectsPosition;
        }

        private void CalculateBaseMouseLook(Vector2 lookInput)
        {
            var x = lookInput.x * _sensitivity * 0.01f;
            var y = lookInput.y * _sensitivity * 0.01f;
            
            _lookRotation.x = Mathf.Clamp(_lookRotation.x - y, -_xRotationClamp, _xRotationClamp);
            _lookRotation.y += x;
        }

        private void CalculateCameraBob(float deltaTime)
        {
            var velocity = _playerMovement.HorizontalVelocity;
            var speed = velocity.magnitude;

            if (speed < _movementVelocityStopThreshold)
                return;
            
            _bobCycle += deltaTime * speed * _bobFrequency;
            
            var bobX = Mathf.Cos(_bobCycle * 0.5f) * _bobAmplitudeX;
            var bobY = Mathf.Sin(_bobCycle) * _bobAmplitudeY;

            _bobPosition = new Vector3(bobX, bobY, 0);
        }

        private void CalculateMovementTilt(float deltaTime)
        {
            var localVelocity = _orientationTransform.InverseTransformDirection(_playerMovement.HorizontalVelocity);
            var targetTilt = -localVelocity.x * _movementTiltAmount;
            targetTilt = Mathf.Clamp(targetTilt, -_movementTiltClamp, _movementTiltClamp);
            _currentMovementTilt = Mathf.Lerp(_currentMovementTilt, targetTilt, deltaTime * _movementTiltSmoothness);
            _movementTiltRotation = new Vector3(0f, 0f, _currentMovementTilt);
        }

        [Command("set_camera_sens", "Changes camera sensitivity")]
        private void SetSensitivity(float sensitivity)
        {
            _sensitivity = sensitivity;
            Debug.Log("Camera sensitivity changed: " + _sensitivity);
        }

        [Command("set_camera_clamp", "Changes camera X clamp")]
        private void ChangeCameraClamp(float clamp)
        {
            _xRotationClamp = clamp;
            Debug.Log("Camera X clamp changed: " + _xRotationClamp);
        }
    }
}