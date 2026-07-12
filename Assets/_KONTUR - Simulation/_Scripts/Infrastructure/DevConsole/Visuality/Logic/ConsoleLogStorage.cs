using System;
using System.Collections.Generic;

namespace KofeyekToolkit.DevConsole
{
    public static class ConsoleLogStorage
    {
        private static readonly List<ConsoleLog> _logs = new();
        
        public static IEnumerable<ConsoleLog> Logs => _logs;
        public static event Action<ConsoleLog> LogAddedEvent;
        
        public static void AddLog(string message, UnityEngine.LogType type)
        {
            var consoleLog = new ConsoleLog(message, type);
            _logs.Add(consoleLog);
            LogAddedEvent?.Invoke(consoleLog);
        }
    }
}