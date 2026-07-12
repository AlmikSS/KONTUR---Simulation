using System.Text;
using _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain;
using TMPro;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts
{
    public sealed class NpcDebugHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _debugText;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private float _updateInterval = 0.2f;
        [SerializeField] private NpcBrain _npc;
        
        private StringBuilder _sb = new();
        private float _updateTimer;
        private bool _isVisible;

        private void Start()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
            else if (_debugText != null)
                _debugText.enabled = false;
        }
        
        private void Update()
        {
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
            _sb.AppendLine($"<color=green>--- {_npc.gameObject.name} ---</color>");
            _sb.AppendLine($"Is init: {_npc.IsInit}");
            _sb.AppendLine($"State name: {_npc.StateMachine.CurrentState.Name}");
            _sb.AppendLine($"Is player in vision: {_npc.Blackboard.IsPlayerInVision}");
            _sb.AppendLine($"Last update time: {_npc.LastUpdateTime}");
            _sb.AppendLine($"Update timer: {_npc.UpdateTimer:F3}");
            
            _debugText.text = _sb.ToString();
        }

        public void ShowNpcDebug(bool enable)
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
        }
    }
}