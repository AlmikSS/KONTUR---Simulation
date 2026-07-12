using System.Text;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.Input;
using GamePlay.Player;
using KofeyekToolkit.DevConsole;
using KofeyekToolkit.TickSystem;
using TMPro;
using UnityEngine;

namespace Tools.GlobalDebug
{
    public class GlobalDebugHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _debugText;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private float _updateInterval = 0.2f;

        private TickSystem _tickSystem;
        private InputSystem _inputSystem;
        private Transform _playerTransform;
        private PlayerMovement _playerMovement;
        private PlayerCamera _playerCamera;
        private bool _showPlayerInfo;
        private bool _showInputInfo;
        
        private StringBuilder _sb = new();
        private float _updateTimer;
        private bool _isVisible;

        private int _framesCount;
        private float _fpsAccumulator;
        private int _currentFPS;

        private void Start()
        {
            _tickSystem = ServiceLocator.Get<TickSystem>();
            _inputSystem = ServiceLocator.Get<InputSystem>();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            if (_playerTransform != null)
            {
                _playerMovement = _playerTransform.GetComponent<PlayerMovement>();
                _playerCamera = _playerTransform.GetComponent<PlayerCamera>();
            }

            if (_panelRoot != null)
                _panelRoot.SetActive(false);
            else if (_debugText != null)
                _debugText.enabled = false;
        }

        private void Update()
        {
            _framesCount++;
            _fpsAccumulator += Time.unscaledDeltaTime;
            
            if (_fpsAccumulator >= _updateInterval)
            {
                _currentFPS = Mathf.RoundToInt(_framesCount / _fpsAccumulator);
                _framesCount = 0;
                _fpsAccumulator = 0f;
            }

            if (!_isVisible)
                return;

            _updateTimer += Time.unscaledDeltaTime;
            if (_updateTimer >= _updateInterval)
            {
                UpdateDebugText();
                _updateTimer = 0f;
            }
        }

        private void UpdateDebugText()
        {
            if (_debugText == null) return;

            _sb.Clear();

            var frameTimeMs = Time.unscaledDeltaTime * 1000f;
            var allocatedMemoryMB = System.GC.GetTotalMemory(false) / 1048576f;

            _sb.AppendLine("<color=green>--- TECHNICAL ---</color>");
            _sb.AppendLine($"FPS: {_currentFPS} ({frameTimeMs:F1} ms)");
            _sb.AppendLine($"Memory: {allocatedMemoryMB:F1} MB");
            _sb.AppendLine();

            _sb.AppendLine("<color=green>--- TICKS ---</color>");
            if (_tickSystem != null)
            {
                _sb.AppendLine($"Target: {_tickSystem.TargetTickRate} Hz");
                _sb.AppendLine($"Real: {_tickSystem.RealTickRate} Hz");
                _sb.AppendLine($"Execution Time: {_tickSystem.TickExecutionTimeMs:F2} ms");
            }
            else
            {
                _sb.AppendLine("TickSystem not found.");
            }
            _sb.AppendLine();

            if (_showInputInfo)
            {
                _sb.AppendLine("<color=green>--- INPUT ---</color>");
                if (_inputSystem != null)
                {
                    var snap = _inputSystem.Snapshot;
                    _sb.AppendLine($"Context: {snap.Context}");
                    _sb.AppendLine($"Move Input: [X: {snap.MoveInput.x:F1}, Y: {snap.MoveInput.y:F1}]");
                    _sb.AppendLine($"Look Input: [X: {snap.LookInput.x:F1}, Y: {snap.LookInput.y:F1}]");
                }
                else
                {
                    _sb.AppendLine("InputSystem not found.");
                }

                _sb.AppendLine();
            }

            if (_showPlayerInfo)
            {
                _sb.AppendLine("<color=green>--- PLAYER ---</color>");

                if (_playerTransform == null)
                    _sb.AppendLine("Player not found.");
                else
                {
                    var position = _playerTransform.position;
                    
                    _sb.AppendLine("<color=blue>-- MOVEMENT --</color>");
                    _sb.AppendLine($"Position: [X: {position.x:F3}, Y: {position.y:F3}, Z: {position.z:F3}]");
                    _sb.AppendLine($"Look rotation: [X: {_playerCamera.LookRotation.x:F3}, Y: {_playerCamera.LookRotation.y:F3}]");
                    _sb.AppendLine($"Horizontal velocity: [X: {_playerMovement.HorizontalVelocity.x:F3}, Z: {_playerMovement.HorizontalVelocity.z:F3}]");
                    _sb.AppendLine($"Vertical velocity: {_playerMovement.VerticalVelocity:F3}");
                    _sb.AppendLine($"IsGrounded: {_playerMovement.IsGrounded}");
                    _sb.AppendLine($"JumpEnabled: {_playerMovement.JumpsEnabled}");
                }
                _sb.AppendLine();
            }
            
            _debugText.text = _sb.ToString();
        }

        [Command("debug", "")]
        private void EnableDebugMode(bool enable)
        {
            CommandExecutor.Execute($"toggle_debug_hud {enable}");
            CommandExecutor.Execute($"toggle_npc_debug {enable}");
            CommandExecutor.Execute($"show_player_info {enable}");
            CommandExecutor.Execute($"show_input_info {enable}");
        }

        [Command("toggle_debug_hud", "Toggles the global debug information panel")]
        private void ToggleDebugHUD(bool enable)
        {
            _isVisible = enable;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(_isVisible);
            }
            else if (_debugText != null)
            {
                _debugText.enabled = _isVisible;
            }
            Debug.Log($"Debug HUD {(_isVisible ? "Enabled" : "Disabled")}");
        }

        [Command("toggle_npc_debug", "Shows npc debug info")]
        private void ToggleNpcDebug(bool enable)
        {
            var npcs = FindObjectsByType<NpcDebugHud>();
            foreach (var npc in npcs)
            {
                npc.ShowNpcDebug(enable);
            }
            
            Debug.Log($"Npc debug {(_isVisible ? "Enabled" : "Disabled")}");
        }

        [Command("show_player_info", "Shows the player information on global debug hud")]
        private void TogglePlayerInfoDebug(bool enable)
        {
            _showPlayerInfo = enable;
            Debug.Log($"Player Info {(_showPlayerInfo ? "Enabled" : "Disabled")}");
        }
        
        [Command("show_input_info", "Shows the Input information on global debug hud")]
        private void ToggleInputInfoDebug(bool enable)
        {
            _showInputInfo = enable;
            Debug.Log($"Input Info {(_showPlayerInfo ? "Enabled" : "Disabled")}");
        }
    }
}