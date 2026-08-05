using UnityEngine;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.Events;
using _KONTUR___Simulation._Scripts;

namespace GamePlay.Enemy.Audio
{
    public class NpcBreathingAudio : MonoBehaviour, ISpawnable, IDespawnable
    {
        [Header("Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _breathingClip;
        [SerializeField] private float _volume = 1.5f;
        [SerializeField] private float _normalPitch = 1f;
        [SerializeField] private float _chasePitch = 1.4f;
        [SerializeField] private float _pitchChangeSpeed = 5f;

        private EventBus _eventBus;
        private float _targetPitch;

        public void OnSpawn()
        {
            if (_breathingClip != null && !_audioSource.isPlaying)
            {
                _audioSource.clip = _breathingClip;
                _audioSource.loop = true;
                _audioSource.volume = _volume;
                _audioSource.Play();
            }

            _targetPitch = _normalPitch;
            _audioSource.pitch = _normalPitch;

            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<ChaseStartedEvent>(OnChaseStarted);
            _eventBus.Register<ChaseEndedEvent>(OnChaseEnded);
        }

        public void OnDespawn()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            if (_eventBus != null)
            {
                _eventBus.Unregister<ChaseStartedEvent>(OnChaseStarted);
                _eventBus.Unregister<ChaseEndedEvent>(OnChaseEnded);
            }
        }

        private void OnDestroy()
        {
            OnDespawn();
        }

        private void Update()
        {
            if (_audioSource == null) return;
            
            _audioSource.pitch = Mathf.MoveTowards(_audioSource.pitch, _targetPitch, _pitchChangeSpeed * Time.deltaTime);
        }

        private void OnChaseStarted(ChaseStartedEvent e)
        {
            _targetPitch = _chasePitch;
        }

        private void OnChaseEnded(ChaseEndedEvent e)
        {
            _targetPitch = _normalPitch;
        }
    }
}