using UnityEngine;
using KofeyekToolkit.LifeCycle.Interfaces;
using Random = UnityEngine.Random;

namespace GamePlay.Enemy.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class FootstepAnimationEvent : MonoBehaviour, ISpawnable, IDespawnable
    {
        [Header("Audio paths")]
        [SerializeField] private string _footstepFolderPath = "Audio/Monster/Footsteps";
        
        [Header("Volume & Pitch")]
        [SerializeField] private float _volume = 1f;
        [SerializeField] private float _minPitch = 0.9f;
        [SerializeField] private float _maxPitch = 1.1f;

        private AudioSource _audioSource;
        private AudioClip[] _footstepClips;
        private int _lastFootstepIndex = -1;
        private bool _clipsLoaded;

        public void OnSpawn()
        {
            _audioSource = GetComponent<AudioSource>();
            LoadClips();
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
            
            if (!string.IsNullOrEmpty(_footstepFolderPath))
            {
                _footstepClips = Resources.LoadAll<AudioClip>(_footstepFolderPath);
                Debug.Log($"Loaded {_footstepClips.Length} monster footstep clips from Resources/{_footstepFolderPath}");
            }
            
            _clipsLoaded = true;
        }

        // Этот метод вызывается из анимации через Animation Event
        public void OnFootstep()
        {
            Debug.Log("FOOTSTEP!");

            if (_footstepClips == null || _footstepClips.Length == 0) 
                return;

            if (_audioSource == null)
                return;

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