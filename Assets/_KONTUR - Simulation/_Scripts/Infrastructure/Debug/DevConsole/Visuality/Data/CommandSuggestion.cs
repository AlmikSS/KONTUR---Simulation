namespace KofeyekToolkit.DevConsole
{
    public sealed class CommandSuggestion
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string Usage;

        public CommandSuggestion(string name, string description, string usage)
        {
            Name = name;
            Description = description;
            Usage = usage;
        }
    }
}