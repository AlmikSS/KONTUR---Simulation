using System;
using _KONTUR___Simulation._Scripts;
using GamePlay.Player;
using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Player.Audio
{
    public class PlayerFootstepsEmitter : MonoBehaviour, ISpawnable, IDespawnable, ITickable
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private AudioSource _audioSource;
        
        [Header("Footstep clips")]
        [SerializeField] private AudioClip[] _footstepClips;
        
        [Header("Land clips")]
        [SerializeField] private AudioClip[] _landClips;
        
        [Header("Volume & Pitch by speed")]
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _sprintSpeed = 6f;
        [SerializeField] private float _minVolume = 0.3f;
        [SerializeField] private float _maxVolume = 1f;
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;
        
        [Header("Land sound")]
        [SerializeField] private float _landVolume = 1f;
        [SerializeField] private float _landPitch = 1f;
        
        private int _lastFootstepIndex = -1;
        private bool _wasGrounded;

        public TickPhase Phase => TickPhase.SimulationPhase;

        public void OnCreate()
        {
            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovement>();
            
            if (_playerCamera == null)
                _playerCamera = GetComponentInChildren<PlayerCamera>();
            
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                    _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            _wasGrounded = true;
        }

        public void OnSpawn()
        {
            if (_playerCamera != null)
                _playerCamera.OnFootstep += PlayFootstep;
            
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            if (_playerCamera != null)
                _playerCamera.OnFootstep -= PlayFootstep;
            
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        public void OnDestroyed()
        {
        }

        public void Tick(float deltaTime)
        {
            if (_playerMovement == null)
                return;

            var isGrounded = _playerMovement.IsGrounded;
            
            if (isGrounded && !_wasGrounded)
            {
                PlayLandSound();
            }
            
            _wasGrounded = isGrounded;
        }

        private void PlayFootstep()
        {
            if (_footstepClips == null || _footstepClips.Length == 0)
                return;
            
            if (_playerMovement == null || _audioSource == null)
                return;

            if (!_playerMovement.IsGrounded)
                return;

            var speed = _playerMovement.HorizontalVelocity.magnitude;
            var t = Mathf.InverseLerp(_walkSpeed, _sprintSpeed, speed);
            
            _audioSource.volume = Mathf.Lerp(_maxVolume, _minVolume, t);
            _audioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, t);
            
            var clip = GetRandomClip(_footstepClips, ref _lastFootstepIndex);
            if (clip != null)
                _audioSource.PlayOneShot(clip);
        }

        private void PlayLandSound()
        {
            if (_landClips == null || _landClips.Length == 0)
                return;
            
            if (_audioSource == null)
                return;
            
            _audioSource.volume = _landVolume;
            _audioSource.pitch = _landPitch;
            
            var clip = _landClips[Random.Range(0, _landClips.Length)];
            _audioSource.PlayOneShot(clip);
        }

        private AudioClip GetRandomClip(AudioClip[] clips, ref int lastIndex)
        {
            if (clips.Length == 0)
                return null;
                
            if (clips.Length == 1)
                return clips[0];
            
            int randomIndex;
            int attempts = 0;
            do
            {
                randomIndex = Random.Range(0, clips.Length);
                attempts++;
            } 
            while (randomIndex == lastIndex && clips.Length > 1 && attempts < 10);
            
            lastIndex = randomIndex;
            return clips[randomIndex];
        }
    }
}