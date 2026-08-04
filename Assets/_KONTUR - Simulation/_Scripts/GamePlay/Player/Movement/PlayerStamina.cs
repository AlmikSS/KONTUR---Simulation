using System;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace GamePlay.Player
{
    public sealed class PlayerStamina : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private float _regenCooldown;
        [SerializeField] private float _maxStamina;
        [SerializeField] private float _criticalLevel;
        [SerializeField] private float _staminaDrainRate;
        [SerializeField] private float _staminaRegenRate;

        private float _currentStamina;
        private float _lastSprintTime;
        private bool _sprintInPreviousTick;
        private bool _isCritical;

        public TickPhase Phase => TickPhase.SimulationPhase;
        public bool CanSprint => _currentStamina > 0;
        public float CurrentStamina => _currentStamina;
        public event Action<float> OnStaminaChanged; 
        public event Action OnStaminaCriticalLevel; 
        
        public void OnSpawn()
        {
            ServiceLocator.Get<TickSystem>().Register(this);
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
                OnStaminaChanged?.Invoke(_currentStamina);

                if (_currentStamina <= _criticalLevel && !_isCritical)
                {
                    _isCritical = true;
                    OnStaminaCriticalLevel?.Invoke();
                }
            }
            else
            {
                if (_sprintInPreviousTick)
                    _lastSprintTime = Time.time;

                if (Time.time - _lastSprintTime >= _regenCooldown)
                {
                    _currentStamina = Mathf.Clamp(_currentStamina + _staminaRegenRate, 0f, _maxStamina);
                    OnStaminaChanged?.Invoke(_currentStamina);
                    
                    if (_currentStamina >= _criticalLevel && _isCritical)
                        _isCritical = false;
                }
            }
            
            _sprintInPreviousTick = PlayerContext.IsSprint;
        }
    }
}