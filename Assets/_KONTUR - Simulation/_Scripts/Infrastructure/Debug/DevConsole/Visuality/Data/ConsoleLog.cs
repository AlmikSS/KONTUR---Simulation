using UnityEngine;

namespace KofeyekToolkit.DevConsole
{
    public class ConsoleLog
    {
        public readonly string Message;
        public readonly LogType Type;

        public ConsoleLog(string message, LogType type)
        {
            Message = message;
            Type = type;
        }
    }
}