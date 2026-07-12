using System.Reflection;

namespace KofeyekToolkit.DevConsole
{
    public sealed class ConsoleCommand
    {
        public readonly string Name;
        public readonly string Description;
        public readonly MethodInfo Method;
        public readonly object Target;
        public readonly ParameterInfo[] Parameters;

        public ConsoleCommand(string name, string description, MethodInfo method, object target, ParameterInfo[] parameters)
        {
            Name = name;
            Description = description;
            Method = method;
            Target = target;
            Parameters = parameters;
        }
    }
}