using UnityEngine;
using KofeyekToolkit.LifeCycle.Interfaces;
using Random = UnityEngine.Random;

namespace GamePlay.Enemy.Audio
{
    public class FootstepAnimationEvent : MonoBehaviour, ISpawnable, IDespawnable
    {
        [Header("Manual Assignment (Alternative)")]
        // [SerializeField] private AudioClip[] _manualFootstepClips;
        [SerializeField] private AudioSource _audioSource;

        [Header("Resource Path")]
        [SerializeField] private string _footstepFolderPath = "Audio/Monster/Footsteps";
        
        [Header("Volume & Pitch")]
        [SerializeField] private float _volume = 1f;
        [SerializeField] private float _minPitch = 0.9f;
        [SerializeField] private float _maxPitch = 1.1f;

        private AudioClip[] _footstepClips;
        private int _lastFootstepIndex = -1;
        private bool _clipsLoaded = false;

        public void OnSpawn()
        {
            if (!_clipsLoaded)
            {
                LoadClips();
            }
        }

        public void OnDespawn()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        private void LoadClips()
        {
            if (_clipsLoaded) 
                return;

            // if (_manualFootstepClips != null && _manualFootstepClips.Length > 0)
            // {
            //     _footstepClips = _manualFootstepClips;
            //     _clipsLoaded = true;
            //     return;
            // }
            
            if (!string.IsNullOrEmpty(_footstepFolderPath))
            {
                _footstepClips = Resources.LoadAll<AudioClip>(_footstepFolderPath);
                Debug.Log($"Loaded {_footstepClips.Length} monster footstep clips from Resources/{_footstepFolderPath}");
                _clipsLoaded = true;
                return;
            }
            
            Debug.LogWarning($"Cannot load footstep sound from path: Resources/{_footstepFolderPath}");
        }

        public void OnFootstep()
        {
            if (_footstepClips == null || _footstepClips.Length == 0)
            {
                Debug.LogWarning($"Footsteps audio folder is null or empty Resources/{_footstepFolderPath}");
                return;
            }

            if (_audioSource == null)
            {
                Debug.LogWarning("Audio source is null");
                return;
            }

            _audioSource.volume = _volume;
            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);

            var clip = GetRandomClip(_footstepClips, ref _lastFootstepIndex);
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
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