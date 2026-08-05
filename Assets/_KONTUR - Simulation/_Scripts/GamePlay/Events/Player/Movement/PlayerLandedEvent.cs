using GamePlay.Player;
using KofeyekToolkit.Events;

public sealed class PlayerLandedEvent : IGameEvent
{
    public readonly float Y;
    public readonly float Distance;

    public PlayerLandedEvent(float y, float distance)
    {
        Y = y;
        Distance = distance;
    }
}