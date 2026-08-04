using System;
using System.Collections.Generic;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class Animator : MonoBehaviour, ITickable, ISpawnable, IDespawnable
    {
        [SerializeField] private GameObject _animationAgent;
        [SerializeField] private float _defaultBlendTime = 0.15f;
        [SerializeField] private float _defaultFadeOutTime = 0.15f;
        [SerializeField] private float _defaultSpeed = 1f;
        [SerializeField] private List<AnimationEntry> _animations = new List<AnimationEntry>();
        
        private UnityEngine.Animator _unityAnimator;
        private Dictionary<string, AnimationClip> _clipCache;
        private Dictionary<string, float> _baseSpeedCache;
        private string _currentKey;
        private float _currentSpeed;
        private float _targetSpeed;
        private float _targetBlendTime;
        private bool _isCrossfading;
        private bool _isFadingOut;
        private float _fadeOutTimer;
        private float _fadeOutDuration;
        private bool _isPaused;
        private bool _isInitialized;
        
        private const int DEFAULT_LAYER = 0;
        
        public TickPhase Phase => TickPhase.SimulationPhase;
        public string CurrentAnimation => _currentKey;
        public float CurrentSpeed => _currentSpeed;
        public bool IsPlaying => _isInitialized && _unityAnimator != null && _unityAnimator.enabled;
        public bool IsPaused => _isPaused;
        
        public void OnSpawn()
        {
            if (_isInitialized) return;
            
            BuildCache();
            
            if (_animationAgent == null)
            {
                Debug.LogError($"[Animator] AnimationAgent is null on {gameObject.name}");
                return;
            }
            
            _unityAnimator = _animationAgent.GetComponent<UnityEngine.Animator>();
            if (_unityAnimator == null)
            {
                Debug.LogError($"[Animator] No UnityEngine.Animator on {_animationAgent.name}");
                return;
            }
            
            _unityAnimator.enabled = true;
            _unityAnimator.speed = 1f;
            _currentSpeed = _defaultSpeed;
            _targetSpeed = _defaultSpeed;
            _isPaused = false;
            _isCrossfading = false;
            _isFadingOut = false;
            _fadeOutTimer = 0f;
            
            if (_clipCache.Count > 0)
            {
                foreach (var pair in _clipCache)
                {
                    Play(pair.Key, _defaultSpeed, 0f);
                    break;
                }
            }
            
            _isInitialized = true;
            ServiceLocator.Get<TickSystem>().Register(this);
        }
        
        public void OnDespawn()
        {
            if (_unityAnimator != null)
                _unityAnimator.enabled = false;
            
            _isInitialized = false;
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }
        
        public void Tick(float deltaTime)
        {
            if (!_isInitialized || _unityAnimator == null || !_unityAnimator.enabled) return;
            
            if (_isFadingOut)
            {
                _fadeOutTimer += deltaTime;
                float progress = Mathf.Clamp01(_fadeOutTimer / _fadeOutDuration);
                _unityAnimator.SetLayerWeight(DEFAULT_LAYER, 1f - progress);
                
                if (progress >= 1f)
                {
                    _isFadingOut = false;
                    _unityAnimator.SetLayerWeight(DEFAULT_LAYER, 0f);
                    _unityAnimator.StopPlayback();
                }
                return;
            }
            
            if (_isCrossfading)
            {
                _targetBlendTime -= deltaTime;
                if (_targetBlendTime <= 0f)
                {
                    _isCrossfading = false;
                    _targetBlendTime = 0f;
                }
            }
            
            if (!_isPaused && !Mathf.Approximately(_currentSpeed, _targetSpeed))
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, deltaTime * 10f);
                _unityAnimator.speed = _currentSpeed;
            }
        }
        
        private void BuildCache()
        {
            _clipCache = new Dictionary<string, AnimationClip>();
            _baseSpeedCache = new Dictionary<string, float>();
            
            if (_animations == null || _animations.Count == 0)
            {
                Debug.LogWarning($"[Animator] No animations configured on {gameObject.name}");
                return;
            }
            
            foreach (var entry in _animations)
            {
                if (!entry.IsValid)
                {
                    Debug.LogWarning($"[Animator] Invalid entry: '{entry.AnimationName}' on {gameObject.name}");
                    continue;
                }
                
                if (_clipCache.ContainsKey(entry.AnimationName))
                {
                    Debug.LogWarning($"[Animator] Duplicate key: '{entry.AnimationName}' on {gameObject.name}");
                    continue;
                }
                
                _clipCache.Add(entry.AnimationName, entry.Clip);
                _baseSpeedCache.Add(entry.AnimationName, entry.Speed);
            }
        }
        
        public void Play(string key, float speedMultiplier = 1f, float blendTime = -1f)
        {
            if (!_isInitialized || _unityAnimator == null) return;
            
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[Animator] Play called with null or empty key on {gameObject.name}");
                return;
            }
            
            if (!_clipCache.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"[Animator] Animation '{key}' not found on {gameObject.name}");
                return;
            }
            
            if (_isFadingOut)
            {
                _isFadingOut = false;
                _unityAnimator.SetLayerWeight(DEFAULT_LAYER, 1f);
            }
            
            float baseSpeed = GetBaseSpeed(key);
            float finalSpeed = baseSpeed * Mathf.Max(0f, speedMultiplier);
            
            if (blendTime < 0f)
                blendTime = _defaultBlendTime;
            
            if (blendTime > 0f)
            {
                _unityAnimator.CrossFade(key, blendTime, DEFAULT_LAYER);  // ← key, а не clip.name
                _isCrossfading = true;
                _targetBlendTime = blendTime;
            }
            else
            {
                _unityAnimator.Play(key, DEFAULT_LAYER, 0f);  // ← key, а не clip.name
                _isCrossfading = false;
                _targetBlendTime = 0f;
            }
            
            _currentKey = key;
            SetSpeed(finalSpeed);
        }
        
        public void Stop(float fadeOutTime = -1f)
        {
            if (!_isInitialized || _unityAnimator == null) return;
            
            if (fadeOutTime < 0f)
                fadeOutTime = _defaultFadeOutTime;
            
            if (fadeOutTime > 0f)
            {
                _isFadingOut = true;
                _fadeOutTimer = 0f;
                _fadeOutDuration = fadeOutTime;
            }
            else
            {
                _unityAnimator.SetLayerWeight(DEFAULT_LAYER, 0f);
                _unityAnimator.StopPlayback();
                _isFadingOut = false;
            }
        }
        
        public void SetSpeed(float speed)
        {
            if (!_isInitialized || _unityAnimator == null) return;
            
            _targetSpeed = Mathf.Max(0f, speed);
            if (!_isPaused && !_isFadingOut)
                _unityAnimator.speed = _targetSpeed;
        }
        
        public void SetSpeedInstant(float speed)
        {
            if (!_isInitialized || _unityAnimator == null) return;
            
            _targetSpeed = Mathf.Max(0f, speed);
            _currentSpeed = _targetSpeed;
            if (!_isPaused && !_isFadingOut)
                _unityAnimator.speed = _currentSpeed;
        }
        
        public bool IsPlayingAnimation(string key)
        {
            if (!_isInitialized || _unityAnimator == null || string.IsNullOrEmpty(key)) return false;
            
            var state = _unityAnimator.GetCurrentAnimatorStateInfo(DEFAULT_LAYER);
            return state.IsName(key) || state.IsName(key + "_Layer");
        }
        
        public bool IsAnimationFinished()
        {
            if (!_isInitialized || _unityAnimator == null) return true;
            return _unityAnimator.GetCurrentAnimatorStateInfo(DEFAULT_LAYER).normalizedTime >= 1f;
        }
        
        public float GetProgress()
        {
            if (!_isInitialized || _unityAnimator == null) return 0f;
            return _unityAnimator.GetCurrentAnimatorStateInfo(DEFAULT_LAYER).normalizedTime % 1f;
        }
        
        public float GetRemainingTime()
        {
            if (!_isInitialized || _unityAnimator == null || string.IsNullOrEmpty(_currentKey)) return 0f;
            if (!_clipCache.TryGetValue(_currentKey, out var clip)) return 0f;
            
            var state = _unityAnimator.GetCurrentAnimatorStateInfo(DEFAULT_LAYER);
            float progress = state.normalizedTime % 1f;
            float remaining = (1f - progress) / Mathf.Max(state.speed * _currentSpeed, 0.01f);
            return remaining * clip.length;
        }
        
        public void Pause()
        {
            if (!_isInitialized || _unityAnimator == null || _isFadingOut) return;
            
            _isPaused = true;
            _unityAnimator.speed = 0f;
        }
        
        public void Resume()
        {
            if (!_isInitialized || _unityAnimator == null || _isFadingOut) return;
            
            _isPaused = false;
            _unityAnimator.speed = _currentSpeed;
        }
        
        public bool HasAnimation(string key)
        {
            return _clipCache != null && _clipCache.ContainsKey(key);
        }
        
        public AnimationClip GetClip(string key)
        {
            if (_clipCache == null) return null;
            _clipCache.TryGetValue(key, out var clip);
            return clip;
        }
        
        public float GetBaseSpeed(string key)
        {
            if (_baseSpeedCache == null) return 1f;
            if (_baseSpeedCache.TryGetValue(key, out var speed))
                return speed;
            return 1f;
        }
        
        public string[] GetAllKeys()
        {
            if (_clipCache == null) return Array.Empty<string>();
            var keys = new string[_clipCache.Count];
            _clipCache.Keys.CopyTo(keys, 0);
            return keys;
        }
        
        [ContextMenu("Log All Animations")]
        private void LogAllAnimations()
        {
            if (_clipCache == null)
            {
                Debug.LogWarning("[Animator] Cache not built yet");
                return;
            }
            
            Debug.Log($"[Animator] {gameObject.name} has {_clipCache.Count} animations:");
            foreach (var pair in _clipCache)
            {
                float baseSpeed = GetBaseSpeed(pair.Key);
                Debug.Log($"  - {pair.Key}: {pair.Value.name} ({pair.Value.length}s) baseSpeed: {baseSpeed}x");
            }
        }
    }
}