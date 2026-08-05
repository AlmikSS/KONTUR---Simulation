using System;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using KofeyekToolkit.Events;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace GamePlay.Player
{
    public sealed class PlayerStamina : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private float _regenCooldown;
        [SerializeField] private float _maxStamina;
        [SerializeField] private float _staminaDrainRate;
        [SerializeField] private float _staminaRegenRate;

        private float _currentStamina;
        private float _lastSprintTime;
        private bool _sprintInPreviousTick;
        private EventBus _eventBus;

        public TickPhase Phase => TickPhase.SimulationPhase;
        public bool CanSprint => _currentStamina > 0;
        public float MaxStamina => _maxStamina;
        public float CurrentStamina => _currentStamina;
        
        public void OnSpawn()
        {
            ServiceLocator.Get<TickSystem>().Register(this);
            _eventBus = ServiceLocator.Get<EventBus>();
            _currentStamina = _maxStamina;
        }
        
        public void OnDespawn()
        {
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }
        
        public void Tick(float deltaTime)
        {
            if (PlayerContext.IsSprint)
            {
                _currentStamina = Mathf.Clamp(_currentStamina - _staminaDrainRate, 0f, _maxStamina);
                _eventBus.Invoke(new OnStaminaChangedEvent(_currentStamina, this));
            }
            else
            {
                if (_sprintInPreviousTick)
                    _lastSprintTime = Time.time;

                if (Time.time - _lastSprintTime >= _regenCooldown)
                {
                    _currentStamina = Mathf.Clamp(_currentStamina + _staminaRegenRate, 0f, _maxStamina);
                    _eventBus.Invoke(new OnStaminaChangedEvent(_currentStamina, this));
                }
            }
            
            _sprintInPreviousTick = PlayerContext.IsSprint;
        }
    }
}