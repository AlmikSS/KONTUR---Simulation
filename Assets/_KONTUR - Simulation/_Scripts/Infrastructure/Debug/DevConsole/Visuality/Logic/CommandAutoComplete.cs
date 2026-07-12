using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KofeyekToolkit.DevConsole
{
    public static class CommandAutoComplete
    {
        public static List<CommandSuggestion> GetSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var suggestions = new List<CommandSuggestion>();
            var parts = input.Split(' ');
            var commandName = parts[0];

            if (parts.Length == 1 && !input.EndsWith(' '))
            {
                var filtered = CommandsRegistry.Commands.Values.
                    Where(c => c.Name.StartsWith(commandName));
                
                foreach (var cmd in filtered)
                    suggestions.Add(BuildSuggestion(cmd));

                return suggestions;
            }
            
            if (CommandsRegistry.TryGetCommand(commandName, out var command))
                suggestions.Add(BuildSuggestion(command));
            
            return suggestions;
        }

        public static int GetCurrentArgumentIndex(string input)
        {
            if (string.IsNullOrEmpty(input))
                return -1;
            
            var parts = input.Split(" ");
            if (parts.Length <= 1)
                return 0;
            
            return parts.Length - 2;
        }
        
        private static CommandSuggestion BuildSuggestion(ConsoleCommand command)
        {
            var usage = new StringBuilder();
            usage.Append(command.Name);

            foreach (var param in command.Parameters)
            {
                usage.Append(" <");
                usage.Append(GetFriendlyTypeName(param.ParameterType));
                usage.Append(">");
            }

            return new CommandSuggestion(
                command.Name,
                command.Description,
                usage.ToString());
        }
        
        public static string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(Vector2)) return "x,y";
            if (type == typeof(Vector3)) return "x,y,z";

            if (type.IsEnum)
                return $"enum:{type.Name}";

            return type.Name;
        }
    }
}