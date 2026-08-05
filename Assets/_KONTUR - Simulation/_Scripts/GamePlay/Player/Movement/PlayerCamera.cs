using System;
using System.Collections;
using UnityEngine;
using Core.Input;
using TriInspector;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.LifeCycle.Interfaces;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using KofeyekToolkit.Events;

namespace GamePlay.Player
{
    public enum FovChangeType
    {
        Set,
        SetImmediate,
        Add,
        Multiply
    }

    public class PlayerCamera : MonoBehaviour, ISpawnable, IDespawnable
    {
        [Title("Dependencies")]
        [SerializeField] private Camera _camera;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private Transform _lookRoot;
        [SerializeField] private Transform _effectsRoot;
        
        [Title("Base options")]
        [SerializeField, Slider(0f, 100f)] private float _sensitivity;
        [SerializeField, Slider(0, 90f)] private float _xRotationClamp;
        [SerializeField, Slider(0, 1f)] private float _movementVelocityStopThreshold;
        [SerializeField] private float _baseFov = 90f;
        [SerializeField] private float _fovSmooth = 8f;
        
        [Title("Camera bob options")]
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeX;
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeY;
        [SerializeField, Slider(0, 5)] private float _bobFrequency;
        
        [Title("Movement tilt options")]
        [SerializeField, Slider(0, 15)] private float _movementTiltAmount;
        [SerializeField, Slider(0, 15)] private float _movementTiltSmoothness;
        [SerializeField, Slider(0, 30)] private float _movementTiltClamp;

        [Title("Jump & Landing spring effects")]
        [SerializeField] private float _jumpKickAmount = 3f;
        [SerializeField] private float _landKickAmount = -8f;
        [SerializeField] private float _springStiffness = 50f;
        [SerializeField] private float _springDamping = 8f;

        [Header("Landing Z-Tilt Spring")]
        [SerializeField] private float _landTiltMultiplier = 5f;
        [SerializeField] private float _landImpactMultiplier = 5f;
        [SerializeField] private float _fallHeightImpactMultiplier = 2f;
        [SerializeField] private float _zVelocityImpulseMultiplier = 5f;
        
        
        [Title("Commands options")]
        [SerializeField] private float _lookAtSpeed;
        [SerializeField] private float _lookAtThreshold;

        public event Action OnFootstep;
        public Vector3 LookRotation => _lookRotation;

        private float _currentFov;
        private float _targetFov;

        // spring
        private float _springZVelocity;
        private float _springZPosition;
        private float _springVelocity;
        private float _springPosition;
        private bool _wasGrounded;

        private Coroutine _lookRoutine;
        private InputSystem _inputSystem;
        private EventBus _eventBus;
        private Vector3 _lookRotation;
        private Vector3 _movementTiltRotation;
        private Vector3 _bobPosition;
        private float _bobCycle;
        private float _currentMovementTilt;
        private bool _isBlocked;

        private Vector3 _heartbeatExtraPosition;
        private Vector3 _heartbeatExtraRotation;

        public void OnSpawn()
        {
            _inputSystem = ServiceLocator.Get<InputSystem>();
            _eventBus = ServiceLocator.Get<EventBus>();

            _currentFov = _baseFov;
            _targetFov = _baseFov;
            _camera.fieldOfView = _baseFov;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _eventBus.Register<PlayerFallingStartedEvent>(OnPlayerFallingStarted);
            _eventBus.Register<PlayerLandedEvent>(OnPlayerLanded);

            ChangeFov(120, FovChangeType.SetImmediate);
            ChangeFov(_baseFov, FovChangeType.Set);
        }

        public void OnDespawn()
        {
            _inputSystem = null;
            _eventBus = null;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            _eventBus.Unregister<PlayerFallingStartedEvent>(OnPlayerFallingStarted);
            _eventBus.Unregister<PlayerLandedEvent>(OnPlayerLanded);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unregister<PlayerFallingStartedEvent>(OnPlayerFallingStarted);
                _eventBus.Unregister<PlayerLandedEvent>(OnPlayerLanded);
            }
        }
        
        private void LateUpdate()
        {
            if (_inputSystem == null || _inputSystem.Context != InputContext.GamePlay || !enabled)
                return;

            var lookInput = _inputSystem.CurrentLookInput;
            var deltaTime = Time.deltaTime;
            
            CalculateBaseMouseLook(lookInput);
            CalculateCameraBob(deltaTime);
            CalculateMovementTilt(deltaTime);
            HandleJumpSpring(deltaTime);
            
            var effectsRotation = _movementTiltRotation + new Vector3(_springPosition, 0f, _springZPosition) + _heartbeatExtraRotation;
            var effectsPosition = _bobPosition + _heartbeatExtraPosition;
            
            _heartbeatExtraPosition = Vector3.zero;
            _heartbeatExtraRotation = Vector3.zero;

            UpdateFov(deltaTime);
            
            _orientationTransform.rotation = Quaternion.Euler(0f, _lookRotation.y, 0f);
            
            if (!_isBlocked)
            {
                _lookRoot.localRotation = Quaternion.Euler(_lookRotation);
                _effectsRoot.localRotation = Quaternion.Euler(effectsRotation);
                _effectsRoot.localPosition = effectsPosition;
            }
        }
        
        public void ApplyHeartbeatFov(float fovKick, float intensity)
        {
            ChangeFov(_baseFov + Mathf.Lerp(0f, fovKick, intensity), FovChangeType.SetImmediate);
            ChangeFov(_baseFov, FovChangeType.Set);
        }

        public void ApplyHeartbeatTransforms(Vector3 positionOffset, Vector3 rotationOffset)
        {
            _heartbeatExtraPosition = positionOffset;
            _heartbeatExtraRotation = rotationOffset;
        }

        public void ChangeFov(float value, FovChangeType type)
        {
            switch (type)
            {
                case FovChangeType.Set:
                    _targetFov = value;
                    break;
                case FovChangeType.SetImmediate:
                    _targetFov = value;
                    _currentFov = value;
                    _camera.fieldOfView = value;
                    break;
                case FovChangeType.Add:
                    _targetFov += value;
                    break;
                case FovChangeType.Multiply:
                    _targetFov *= value;
                    break;
            }
        }

        private void UpdateFov(float deltaTime)
        {
            _currentFov = Mathf.Lerp(_currentFov, _targetFov, deltaTime * _fovSmooth);
            _camera.fieldOfView = _currentFov;
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
            
            DetectFootstep();
        }

        private void CalculateMovementTilt(float deltaTime)
        {
            var localVelocity = _orientationTransform.InverseTransformDirection(_playerMovement.HorizontalVelocity);
            var targetTilt = -localVelocity.x * _movementTiltAmount;
            targetTilt = Mathf.Clamp(targetTilt, -_movementTiltClamp, _movementTiltClamp);
            _currentMovementTilt = Mathf.Lerp(_currentMovementTilt, targetTilt, deltaTime * _movementTiltSmoothness);
            _movementTiltRotation = new Vector3(0f, 0f, _currentMovementTilt);
        }

        private void HandleJumpSpring(float deltaTime)
        {
            var isGrounded = _playerMovement.IsGrounded;
            
            if (!isGrounded && _wasGrounded)
            {
                _springVelocity += _jumpKickAmount;
            }
            
            if (isGrounded && !_wasGrounded)
            {
                _springVelocity += _landKickAmount;
            }
            
            var springForce = -_springStiffness * _springPosition;
            var dampingForce = -_springDamping * _springVelocity;
            
            _springVelocity += (springForce + dampingForce) * deltaTime;
            _springPosition += _springVelocity * deltaTime;
            
            var springZForce = -_springStiffness * _springZPosition;
            var dampingZForce = -_springDamping * _springZVelocity;
            
            _springZVelocity += (springZForce + dampingZForce) * deltaTime;
            _springZPosition += _springZVelocity * deltaTime;

            if (Mathf.Abs(_springPosition) < 0.01f && Mathf.Abs(_springVelocity) < 0.01f)
            {
                _springPosition = 0f;
                _springVelocity = 0f;
            }

            if (Mathf.Abs(_springZPosition) < 0.01f && Mathf.Abs(_springZVelocity) < 0.01f)
            {
                _springZPosition = 0f;
                _springZVelocity = 0f;
            }
            
            _wasGrounded = isGrounded;
        }

        private void OnPlayerFallingStarted(PlayerFallingStartedEvent ev)
        {
            _springVelocity += _jumpKickAmount;
        }

        private void OnPlayerLanded(PlayerLandedEvent ev)
        {
            var landKick = _landKickAmount - (ev.Distance * _landImpactMultiplier);
            _springVelocity += landKick;

            float tiltDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float targetTiltAngle = ev.Distance * _landTiltMultiplier * tiltDirection;

            _springZPosition += targetTiltAngle;
            _springZVelocity += targetTiltAngle * _zVelocityImpulseMultiplier; 
        }

        private void DetectFootstep()
        {
            var prevSin = Mathf.Sin(_bobCycle - Time.deltaTime * _playerMovement.HorizontalVelocity.magnitude * _bobFrequency);
            var currSin = Mathf.Sin(_bobCycle);

            if (prevSin < -0.9f && currSin >= -0.9f)
                OnFootstep?.Invoke();
        }

        [Command("block_camera", "Blocks player camera rotation")]
        public void BlockCamera(bool block)
        {
            _isBlocked = block;
        }

        [Command("look_at", "Rotate camera to euler angles")]
        public void LookAt(Vector3 point)
        {
            if (_lookRoutine != null)
                StopCoroutine(_lookRoutine);
            
            _lookRoutine = StartCoroutine(LookAtRoutine(point));
        }

        private IEnumerator LookAtRoutine(Vector3 point)
        {
            BlockCamera(true);
            var targetRotation = Quaternion.Euler(point);
            while (Quaternion.Angle(_lookRoot.rotation, targetRotation) > _lookAtThreshold)
            {
                _lookRoot.rotation = Quaternion.Slerp(_lookRoot.rotation, targetRotation, _lookAtSpeed * Time.deltaTime);
                yield return null;
            }
            _lookRoot.rotation = targetRotation;
            _lookRotation = targetRotation.eulerAngles;
            BlockCamera(false);
        }

        [Command("set_camera_sens", "Changes camera sensitivity")]
        private void SetSensitivity(float sensitivity)
        {
            _sensitivity = sensitivity;
        }

        [Command("set_camera_clamp", "Changes camera X clamp")]
        private void ChangeCameraClamp(float clamp)
        {
            _xRotationClamp = clamp;
        }
    }
}