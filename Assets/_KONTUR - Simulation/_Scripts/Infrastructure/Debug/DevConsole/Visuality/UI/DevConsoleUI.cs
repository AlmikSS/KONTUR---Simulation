using System.Collections.Generic;
using System.Text;
using TMPro;
using TriInspector;
using UnityEngine;

namespace KofeyekToolkit.DevConsole
{
    public class DevConsoleUI : MonoBehaviour
    {
        [Title("DevConsoleUI")]
        [SerializeField] private GameObject _root;
        [SerializeField] private int _commandHistoryLimit;
        [SerializeField] private int _logHistoryLimit;
        
        [Title("Log")]
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _logPrefab;
        [SerializeField] private Transform _logContent;
        
        [Title("Suggestions")]
        [SerializeField] private Transform _suggestionsContent;
        [SerializeField] private GameObject _suggestionsRoot;
        [SerializeField] private TMP_Text _suggestionsPrefab;
        
        private readonly List<string> _commandsHistory = new();
        private int _currentCommandIndex = -1;
        private string _currentInputBuffer;
        
        public bool IsOpened { get; private set; }
        
        public void Open()
        {
            _root.SetActive(true);
            _inputField.ActivateInputField();
            IsOpened = true;
        }

        public void Close()
        {
            _root.SetActive(false);
            IsOpened = false;
        }

        private void Start()
        {
            _inputField.onValueChanged.AddListener(OnInputChanged);
            _inputField.onSubmit.AddListener(OnInputSubmit);
            ConsoleLogStorage.LogAddedEvent += OnLogAdded;
        }

        private void OnDestroy()
        {
            _inputField.onValueChanged.RemoveListener(OnInputChanged);
            _inputField.onSubmit.RemoveListener(OnInputSubmit);
            ConsoleLogStorage.LogAddedEvent -= OnLogAdded;
        }

        private void Update()
        {
            if (!_inputField.isFocused)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }
        }

        private void NavigateHistory(int direction)
        {
            if (_commandsHistory.Count == 0)
                return;

            if (_currentCommandIndex == -1)
            {
                _currentInputBuffer = _inputField.text;
                _currentCommandIndex = _commandsHistory.Count;
            }

            _currentCommandIndex += direction;
            _currentCommandIndex = Mathf.Clamp(_currentCommandIndex, 0, _commandsHistory.Count);

            if (_currentCommandIndex == _commandsHistory.Count)
            {
                _inputField.text = _currentInputBuffer;
            }
            else
            {
                _inputField.text = _commandsHistory[_currentCommandIndex];
            }

            _inputField.caretPosition = _inputField.text.Length;
        }
        
        private void OnLogAdded(ConsoleLog newLog)
        {
            var text = Instantiate(_logPrefab, _logContent);

            if (_logContent.childCount > _logHistoryLimit)
            {
                Destroy(_logContent.GetChild(0).gameObject);
            }

            var color = newLog.Type switch
            {
                LogType.Warning => "yellow",
                LogType.Error => "red",
                LogType.Exception => "red",
                _ => "white"
            };

            text.SetText($"<color={color}>{newLog.Message}</color>");
        }
        
        private void OnInputSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            CommandExecutor.Execute(input);

            _commandsHistory.Add(input);
            _currentCommandIndex = -1;
            
            if (_commandsHistory.Count > _commandHistoryLimit)
                _commandsHistory.RemoveAt(0);

            _inputField.text = "";
            ClearSuggestions();
        }

        private void ClearSuggestions()
        {
            foreach (Transform s in _suggestionsContent)
                Destroy(s.gameObject);

            _suggestionsRoot.SetActive(false);
        }

        private void OnInputChanged(string input)
        {
            BuildSuggestions(input);
        }

        private void BuildSuggestions(string input)
        {
            ClearSuggestions();
            var suggestions = CommandAutoComplete.GetSuggestions(input);

            if (suggestions == null || suggestions.Count == 0)
            {
                _suggestionsRoot.SetActive(false);
                return;
            }

            _suggestionsRoot.SetActive(true);
            var argIndex = CommandAutoComplete.GetCurrentArgumentIndex(input);
            var parts = input.Split(' ');
            var commandName = parts[0].ToLower();

            foreach (var s in suggestions)
            {
                var usageText = s.Usage;

                if (CommandsRegistry.TryGetCommand(commandName, out var command))
                    usageText = BuildHighlightedUsage(command, argIndex);

                var text = Instantiate(_suggestionsPrefab, _suggestionsContent);
                text.SetText($"<color=#4FC3F7>{usageText}</color>  <color=#888888>{s.Description}</color>");
            }
        }

        private string BuildHighlightedUsage(ConsoleCommand command, int argIndex)
        {
            StringBuilder sb = new();

            sb.Append(command.Name);

            for (var i = 0; i < command.Parameters.Length; i++)
            {
                sb.Append(" ");
                var paramName = $"<{CommandAutoComplete.GetFriendlyTypeName(command.Parameters[i].ParameterType)}>";

                if (i == argIndex)
                    sb.Append($"<color=#FFD54F>{paramName}</color>");
                else
                    sb.Append($"<color=#AAAAAA>{paramName}</color>");
            }

            return sb.ToString();
        }

        [Command("help", "Shows all commands")]
        private void HelpCommand()
        {
            foreach (var command in CommandsRegistry.Commands.Values)
            {
                Debug.Log($"'{command.Name}': {command.Description}'");
            }
        }
    }
}