using _KONTUR___Simulation._Scripts.GamePlay.Interactors;
using KofeyekToolkit.Events;
using UnityEngine;

public sealed class InteractionShownEvent : IGameEvent
{
    public readonly InteractorBase Interactable;
    public readonly Transform FocusPoint;
    public readonly string ActionText;
    public readonly string DescriptionText;
    public readonly Sprite HintSprite;

    public InteractionShownEvent(InteractorBase interactable)
    {
        Interactable = interactable;
        FocusPoint = interactable.FocusPoint;
        ActionText = interactable.ActionText;
        DescriptionText = interactable.DescriptionText;
        HintSprite = interactable.HintSprite;
    }
}