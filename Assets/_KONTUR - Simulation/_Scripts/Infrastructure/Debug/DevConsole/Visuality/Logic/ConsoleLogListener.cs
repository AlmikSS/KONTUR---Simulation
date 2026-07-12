using UnityEngine;

namespace KofeyekToolkit.DevConsole
{
    public class ConsoleLogListener : MonoBehaviour
    {
        private void Awake()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string condition, string stackTrace, LogType logType)
        {
            ConsoleLogStorage.AddLog(condition, logType);
        }
    }
}