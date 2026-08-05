using System.Collections;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using _KONTUR___Simulation._Scripts.Input;
using Core.Input;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace GamePlay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _sprintSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private bool _jumpsEnabled;
        [SerializeField] private float _moveToSpeed;
        [SerializeField] private float _moveToThreshold;
        
        [Header("Gravity & Jump Feel")]
        [SerializeField] private float _minFallHeightToReport = 0.5f;
        [SerializeField] private float _gravityScale = -30f;
        [SerializeField] private float _fallGravityMultiplier;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _hangTimeThreshold = 2f;
        
        private Coroutine _moveToRoutine;
        private CharacterController _cc;
        private Collider[] _cols;
        private InputSystem _inputSystem;
        private EventBus _eventBus;
        private Vector3 _horizontalVelocity;
        private Vector3 _groundNormal = Vector3.up;
        private float _verticalVelocity;
        private float _highestYDuringFall;
        private bool _wasGrounded = true;
        private bool _blocked;
        
        public TickPhase Phase => TickPhase.SimulationPhase;
        public Vector3 HorizontalVelocity => _horizontalVelocity;
        public float VerticalVelocity => _verticalVelocity;
        public bool IsGrounded => _cc.isGrounded;
        public bool JumpsEnabled => _jumpsEnabled;
        public bool IsSprint { get; private set; }

        public void OnSpawn()
        {
            _cc = GetComponent<CharacterController>();
            _cols = GetComponentsInChildren<Collider>();
            _inputSystem = ServiceLocator.Get<InputSystem>();
            _eventBus = ServiceLocator.Get<EventBus>();

            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            _cc = null;
            _cols = null;
            _inputSystem = null;
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }
        
        public void Tick(float deltaTime)
        {
            if (_inputSystem == null || _inputSystem.Snapshot.Context != InputContext.GamePlay || !enabled)
                return;

            var snapshot = _inputSystem.Snapshot;
            var moveInput = _inputSystem.CurrentMoveInput;
            var input = new Vector3(moveInput.x, 0, moveInput.y);
            input = Vector3.ClampMagnitude(input, 1f);

            var worldDirection = _orientationTransform.TransformDirection(input);

            if (_cc.isGrounded)
                worldDirection = Vector3.ProjectOnPlane(worldDirection, _groundNormal).normalized;

            bool isMoving = input.magnitude > 0.01f || _horizontalVelocity.magnitude > 0.1f;
            
            IsSprint = PlayerContext.CanSprint && snapshot.SprintInput && isMoving;

            var targetVelocity = worldDirection * (IsSprint ? _sprintSpeed : _walkSpeed);

            if (!_blocked)
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, _acceleration * deltaTime);

            bool isGrounded = _cc.isGrounded;

            if (!isGrounded && _wasGrounded)
            {
                _highestYDuringFall = transform.position.y;
                _eventBus.Invoke(new PlayerFallingStartedEvent(_highestYDuringFall));
            }
            else if (!isGrounded)
            {
                if (transform.position.y > _highestYDuringFall)
                {
                    _highestYDuringFall = transform.position.y;
                }
            }
            else if (isGrounded && !_wasGrounded)
            {
                float fallHeight = _highestYDuringFall - transform.position.y;

                if (fallHeight >= _minFallHeightToReport)
                {
                    _eventBus.Invoke(new PlayerLandedEvent(transform.position.y, fallHeight));
                }
            }

            _wasGrounded = isGrounded;

            if (_cc.isGrounded)
                _verticalVelocity = -4f;
            else
            {
                float currentGravity = _gravityScale;
                if (_verticalVelocity > 0 && _verticalVelocity < 2f)
                    currentGravity *= 0.5f;
                else if (_verticalVelocity < 0)
                    currentGravity *= _fallGravityMultiplier;

                _verticalVelocity += currentGravity * deltaTime;
            }
            
            if (snapshot.JumpInput)
                Jump();
            
            var finalVelocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
            if (_cc.enabled)
                _cc.Move(finalVelocity * deltaTime);
        }

        private void Jump()
        {
            if (!_cc.isGrounded || !_jumpsEnabled || _blocked)
                return;

            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravityScale);
        }

        [Command("block_movement", "Blocks player movement")]
        public void BlockMovement(bool block)
        {
            _blocked = block;
            
            if (_blocked)
                _horizontalVelocity = Vector3.zero;
            
            Debug.Log("Block movement: " + _blocked);
        }

        [Command("move", "Moves player to point")]
        public void MoveTo(Vector3 point)
        {
            if (_moveToRoutine != null)
                StopCoroutine(_moveToRoutine);
            
            _moveToRoutine = StartCoroutine(MoveToRoutine(point));
            Debug.Log("Move to: " + point);
        }

        private IEnumerator MoveToRoutine(Vector3 point)
        {
            _cc.enabled = false;
            foreach (var col in _cols)
                col.enabled = false;

            while (Vector3.Distance(point, transform.position) > _moveToThreshold)
            {
                transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime * _moveToSpeed);
                yield return null;
            }
            
            _cc.enabled = true;
            foreach (var col in _cols)
                col.enabled = true;
        }

        private void UpdateGroundNormal()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f,
                    Vector3.down,
                    out var hit,
                    1.5f))
            {
                _groundNormal = hit.normal;
            }
            else
            {
                _groundNormal = Vector3.up;
            }
        }

        [Command("set_jump_enable", "Enable/disable jumps")]
        private void SetJumpEnable(bool enable)
        {
            _jumpsEnabled = enable;
            var log = enable ? "enabled" : "disabled";
            Debug.Log("Jump " + log);
        }

        [Command("set_jump_height", "Change player jump height")]
        private void SetJumpHeight(float jumpHeight)
        {
            _jumpHeight = jumpHeight;
            Debug.Log("Jump height changed: " + _jumpHeight);
        }

        [Command("set_player_speed", "Change player speed")]
        private void SetSpeed(float speed)
        {
            _walkSpeed = speed;
            Debug.Log("Player speed changed: " + _walkSpeed);
        }

        [Command("set_player_gravity", "Change player gravity scale")]
        private void SetPlayerGravity(float gravity)
        {
            _gravityScale = gravity;
            Debug.Log("Player gravity changed: " + _gravityScale);
        }
    }
}