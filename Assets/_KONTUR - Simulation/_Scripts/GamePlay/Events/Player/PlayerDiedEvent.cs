using KofeyekToolkit.Events;

public sealed class PlayerDiedEvent : IGameEvent
{
    public readonly PlayerDiedFromType DiedFrom;

    public PlayerDiedEvent(PlayerDiedFromType diedFrom = PlayerDiedFromType.Unknown)
    {
        DiedFrom = diedFrom;
    }
}