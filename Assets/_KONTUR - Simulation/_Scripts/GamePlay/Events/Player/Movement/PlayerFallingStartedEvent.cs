using GamePlay.Player;
using KofeyekToolkit.Events;

public sealed class PlayerFallingStartedEvent : IGameEvent
{
    public readonly float Y;

    public PlayerFallingStartedEvent(float y)
    {
        Y = y;
    }
}