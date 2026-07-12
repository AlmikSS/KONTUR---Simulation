using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KofeyekToolkit.DevConsole
{
    public static class CommandsRegistry
    {
        private static readonly Dictionary<string, ConsoleCommand> _commands = new(); 
        public static IReadOnlyDictionary<string, ConsoleCommand> Commands => _commands;

        public static void RegisterAllCommands()
        {
            _commands.Clear();

            var methods = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(a => a.GetTypes()).
                SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                
                if (attribute == null)
                    continue;

                object target = null;

                if (!method.IsStatic)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(method.DeclaringType))
                        target = UnityEngine.Object.FindAnyObjectByType(method.DeclaringType);
                    else if (method.DeclaringType != null)
                        target = Activator.CreateInstance(method.DeclaringType);
                }

                var command = new ConsoleCommand(
                    attribute.Name,
                    attribute.Description,
                    method,
                    target,
                    method.GetParameters());
                
                _commands.Add(command.Name, command);
            }
        }

        public static bool TryGetCommand(string commandName, out ConsoleCommand command)
        {
            return _commands.TryGetValue(commandName.ToLower(), out command);
        }
    }
}