using System;
using System.Collections.Generic;
using System.Diagnostics;
using _KONTUR___Simulation._Scripts;
using KofeyekToolkit.DevConsole;
using TriInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KofeyekToolkit.TickSystem
{
    public sealed class TickSystem : MonoBehaviour, IService
    {
        [SerializeField] private int _tickRate = 60;

        private readonly SortedDictionary<TickPhase, List<ITickable>> _tickables = new();
        private readonly Queue<ITickable> _registerQueue = new();
        private readonly Queue<ITickable> _unregisterQueue = new();
        
        private int _tickCounter;
        private float _tickInterval;
        private float _accumulator;
        private float _tickTimer;
        private bool _isConstruct;
        private bool _isTicking;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        public int TargetTickRate => _tickRate;
        public int RealTickRate { get; private set; }
        public float TickExecutionTimeMs { get; private set; }
        public float TickInterval => _tickInterval;
        public bool IsTicking => _isTicking;

        public void Initialize()
        {
            SetTickSettings();
            
            foreach (TickPhase phase in Enum.GetValues(typeof(TickPhase)))
            {
                _tickables[phase] = new List<ITickable>();
            }

            _isConstruct = true;
        }
        
        public void StartTicks()
        {
            if (!_isConstruct)
                return;

            _isTicking = true;
            Debug.Log("[TickSystem] Ticks started");
        }
        
        public void StopTicks()
        {
            _isTicking = false;
            Debug.Log("[TickSystem] Ticks stopped");
        }
        
        public void Register(ITickable tickable) => _registerQueue.Enqueue(tickable);
        public void Unregister(ITickable tickable) => _unregisterQueue.Enqueue(tickable);
        
        private void Update()
        {
            if (!_isConstruct)
                return;
            
            _tickTimer += Time.unscaledDeltaTime;
            if (_tickTimer >= 1f)
            {
                RealTickRate = _tickCounter;
                _tickCounter = 0;
                _tickTimer -= 1f;
            }

            _accumulator += Time.deltaTime;
            while (_accumulator >= _tickInterval)
            {
                _stopwatch.Restart();
                
                Tick(_tickInterval);
                _tickCounter++;
                
                _stopwatch.Stop();
                TickExecutionTimeMs = (float)_stopwatch.Elapsed.TotalMilliseconds;
                
                _accumulator -= _tickInterval;
            }
        }

        private void Tick(float deltaTime)
        {
            PruneDestroyedTickables();

            foreach (var tickables in _tickables.Values)
            {
                foreach (var tickable in tickables)
                {
                    if (tickable.Phase is TickPhase.StartPhase or TickPhase.InputPhase or TickPhase.SystemPhase or TickPhase.EndPhase)
                    {
                        tickable.Tick(deltaTime);
                        continue;
                    }
                    
                    if (_isTicking)
                        tickable.Tick(deltaTime);
                }
            }
            
            ReleaseRegisterQueue();
            ReleaseUnregisterQueue();
        }

        private void PruneDestroyedTickables()
        {
            foreach (var phase in _tickables.Keys)
            {
                var tickables = _tickables[phase];

                for (int i = tickables.Count - 1; i >= 0; i--)
                {
                    if (tickables[i] is UnityEngine.Object unityObj && unityObj == null)
                        tickables.RemoveAt(i);
                }
            }
        }
        
        private void ReleaseRegisterQueue()
        {
            while (_registerQueue.Count > 0)
            {
                var newTickable = _registerQueue.Dequeue();
                var phase = newTickable.Phase;
                
                if (_tickables[phase].Contains(newTickable))
                    continue;
                
                _tickables[phase].Add(newTickable);
                Debug.Log($"[TickSystem] Registered tickable {newTickable.GetType().Name}");
            }
        }

        private void ReleaseUnregisterQueue()
        {
            while (_unregisterQueue.Count > 0)
            {
                var tickable = _unregisterQueue.Dequeue();
                var phase = tickable.Phase;
                
                if (!_tickables[phase].Contains(tickable))
                    continue;
                
                _tickables[phase].Remove(tickable);
                Debug.Log($"[TickSystem] Unregistered tickable {tickable.GetType().Name}");
            }
        }
        
        [Button(ButtonSizes.Medium, "Recalculate ticks")]
        private void SetTickSettings()
        {
            _tickInterval = 1f / _tickRate;
            _accumulator = 0f;
        }

        [Command("change_tickrate", "Changes current tick rate in game")]
        private void ChangeTickRate(int tickRate)
        {
            _tickRate = tickRate;
            SetTickSettings();
            Debug.Log("[TickSystem] Tick rate changed");
        }
    }
}