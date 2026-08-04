using System;
using UnityEngine;
using GamePlay.Player;
using System.Collections.Generic;
using KofeyekToolkit.LifeCycle.Interfaces;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    public abstract class InteractorBase : MonoBehaviour, IInteractable, ISpawnable, IDespawnable
    {
        [SerializeField] private Sprite _hint;
        [SerializeField] private Transform _focusPoint;
        [SerializeField] private string _actionText;
        [SerializeField] private string _descriptionText;
        
        public bool HasHint => _hint != null;
        public Sprite HintSprite => _hint;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;
        public virtual string ActionText => _actionText;
        public virtual string DescriptionText => _descriptionText;

        public static HashSet<InteractorBase> Registry { get; } = new();
        
        public virtual void Interact(GameObject interactor) { }

        public virtual void SecondaryInteract(GameObject interactor) { }

        // registry

        public virtual void OnSpawn()
        {
            Registry.Add(this);
        }

        public virtual void OnDespawn()
        {
            Registry.Remove(this);
        }
    }
}