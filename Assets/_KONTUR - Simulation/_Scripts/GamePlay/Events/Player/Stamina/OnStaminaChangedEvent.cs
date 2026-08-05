using GamePlay.Player;
using KofeyekToolkit.Events;

public sealed class OnStaminaChangedEvent : IGameEvent
{
    public readonly float CurrentValue;
    public readonly PlayerStamina Component;

    public OnStaminaChangedEvent(float currentValue, PlayerStamina component)
    {
        CurrentValue = currentValue;
        Component = component;
    }
}