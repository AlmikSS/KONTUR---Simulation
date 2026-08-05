using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using GamePlay.Player;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player.Heartbeat
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class HeartbeatController : MonoBehaviour, ISpawnable, ITickable, IDespawnable
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip _heartbeatClip;
        [SerializeField] private float _maxVolume = 1f;
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private float _minPitch = 0.95f;
        [SerializeField] private float _maxPitch = 1.15f;

        [Header("Heart Rate Settings")]
        [SerializeField] private float _minHeartRate = 60f;
        [SerializeField] private float _maxHeartRate = 180f;

        [Header("Visual Effects Settings")]
        [SerializeField] private float _heartbeatFovKick = 4f;
        [SerializeField] private float _heartbeatShakeAmplitude = 0.015f;
        [SerializeField] private float _heartbeatRollAmplitude = 0.4f;
        [SerializeField] private float _heartbeatNoiseSpeed = 12f;
        [SerializeField] private float _heartbeatKickAmount = 2.5f;
        [SerializeField] private float _heartbeatSpringStiffness = 45f;
        [SerializeField] private float _heartbeatSpringDamping = 10f;

        private AudioSource _audioSource;
        private PlayerCamera _playerCamera;
        
        private float _heartbeatSpringPosition;
        private float _heartbeatSpringVelocity;
        private float _heartbeatNoiseSeed;
        private float _beatTimer;

        public TickPhase Phase => TickPhase.SimulationPhase;
        public float Intensity { get; private set; }

        public void OnSpawn()
        {
            _audioSource = GetComponent<AudioSource>();
            _playerCamera = FindAnyObjectByType<PlayerCamera>();
            _heartbeatNoiseSeed = Random.Range(0f, 1000f);
            
            _audioSource.loop = false; // Теперь лупить будем сами по таймеру ударов
            _audioSource.volume = 0f;
            
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            _audioSource.Stop();
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        private void OnDestroy()
        {
            _audioSource.Stop();
            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            var highestIntensity = 0f;

            foreach (var source in HeartbeatTrigger.Triggers)
            {
                if (source == null) continue;

                var intensity = source.GetIntensity(transform.position);
                if (intensity > highestIntensity)
                    highestIntensity = intensity;
            }
            
            Intensity = Mathf.Lerp(
                Intensity,
                highestIntensity,
                deltaTime * _fadeSpeed);

            _audioSource.volume = Intensity * _maxVolume;

            UpdateSpring(deltaTime);

            if (Intensity > 0f)
            {
                var currentHeartRate = Mathf.Lerp(_minHeartRate, _maxHeartRate, Intensity);
                var beatInterval = 60f / currentHeartRate;

                _audioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, Intensity);

                _beatTimer += deltaTime;
                if (_beatTimer >= beatInterval)
                {
                    _beatTimer = 0f;
                    TriggerHeartbeatBeat();
                }

                if (_playerCamera != null)
                {
                    ApplyVisualEffects();
                }
            }
            else
            {
                _beatTimer = 0f;
                _audioSource.pitch = _minPitch;
            }
        }

        private void TriggerHeartbeatBeat()
        {
            if (_heartbeatClip != null)
                _audioSource.PlayOneShot(_heartbeatClip, Intensity);

            _heartbeatSpringVelocity += _heartbeatKickAmount;

            if (_playerCamera != null)
                _playerCamera.ApplyHeartbeatFov(_heartbeatFovKick, Intensity);
        }

        private void UpdateSpring(float deltaTime)
        {
            var springForce = -_heartbeatSpringStiffness * _heartbeatSpringPosition;
            var dampingForce = -_heartbeatSpringDamping * _heartbeatSpringVelocity;
            
            _heartbeatSpringVelocity += (springForce + dampingForce) * deltaTime;
            _heartbeatSpringPosition += _heartbeatSpringVelocity * deltaTime;
        }

        private void ApplyVisualEffects()
        {
            var t = Time.time * _heartbeatNoiseSpeed;
            var x = (Mathf.PerlinNoise(_heartbeatNoiseSeed, t) - 0.5f) * 2f;
            var y = (Mathf.PerlinNoise(_heartbeatNoiseSeed + 43f, t) - 0.5f) * 2f;
            var roll = (Mathf.PerlinNoise(_heartbeatNoiseSeed + 91f, t) - 0.5f) * 2f;

            Vector3 shakePosition = new Vector3(x, y, 0f) * (_heartbeatShakeAmplitude * Intensity);
            Vector3 shakeRotation = new Vector3(_heartbeatSpringPosition * 0.8f, 0f, roll * (_heartbeatRollAmplitude * Intensity));

            _playerCamera.ApplyHeartbeatTransforms(shakePosition, shakeRotation);
        }
    }
}