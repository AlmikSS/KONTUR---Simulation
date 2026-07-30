using KofeyekToolkit.LifeCycle.Interfaces;
using KofeyekToolkit.TickSystem;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Player.Heartbeat
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class HeartbeatController : MonoBehaviour, ISpawnable, ITickable, IDespawnable
    {
        [SerializeField] private float _maxVolume = 1f;
        [SerializeField] private float _fadeSpeed = 3f;
        
        private AudioSource _audioSource;

        public TickPhase Phase => TickPhase.SimulationPhase;

        public void OnSpawn()
        {
            _audioSource = GetComponent<AudioSource>();
            
            _audioSource.loop = true;
            _audioSource.volume = 0f;
            
            if (!_audioSource.isPlaying)
                _audioSource.Play();
            
            ServiceLocator.Get<TickSystem>().Register(this);
        }

        public void OnDespawn()
        {
            _audioSource.Stop();
            ServiceLocator.Get<TickSystem>().Unregister(this);
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
            
            var targetVolume = highestIntensity * _maxVolume;
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVolume, Time.deltaTime * _fadeSpeed);
        }
    }
}