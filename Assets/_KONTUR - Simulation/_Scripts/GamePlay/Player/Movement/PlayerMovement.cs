using System.Collections;
using System.Threading.Tasks;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using Core.Input;
using KofeyekToolkit.DevConsole;
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
        [SerializeField] private float _acceleration;
        [SerializeField] private float _gravityScale; 
        [SerializeField] private float _jumpHeight;
        [SerializeField] private bool _jumpsEnabled;
        [SerializeField] private float _moveToSpeed;
        [SerializeField] private float _moveToThreshold;
        
        private Coroutine _moveToRoutine;
        private CharacterController _cc;
        private Collider[] _cols;
        private InputSystem _inputSystem;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private bool _blocked;
        
        public TickPhase Phase => TickPhase.SimulationPhase;
        public Vector3 HorizontalVelocity => _horizontalVelocity;
        public float VerticalVelocity => _verticalVelocity;
        public bool IsGrounded => _cc.isGrounded;
        public bool JumpsEnabled => _jumpsEnabled;

        public void OnSpawn()
        {
            _cc = GetComponent<CharacterController>();
            _cols = GetComponentsInChildren<Collider>();
            _inputSystem = ServiceLocator.Get<InputSystem>();
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
            _cc = null;
            _cols = null;
            _inputSystem = null;
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
            var targetVelocity = worldDirection * _walkSpeed;
            if (!_blocked)
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, _acceleration * deltaTime);

            if (_cc.isGrounded)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += _gravityScale * deltaTime;
            
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

            _verticalVelocity = 0f;
            _verticalVelocity += _jumpHeight;
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