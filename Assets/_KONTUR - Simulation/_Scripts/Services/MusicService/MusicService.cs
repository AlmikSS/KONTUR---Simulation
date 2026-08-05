using System.Collections.Generic;
using KofeyekToolkit.Events;
using KofeyekToolkit.TickSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _KONTUR___Simulation._Scripts.Services
{
    public sealed class MusicService : MonoBehaviour, IService, ITickable
    {
        private const float ChaseResumeSilenceThreshold = 7f;
        private const string ChaseTrackKey = "Chase";
        private const string ShelterTrackKey = "Hiding";

        [SerializeField] private List<MusicTrackConfig> _tracks = new();

        private EventBus _eventBus;
        private readonly Dictionary<string, TrackRuntime> _runtimeTracks = new();

        private float _chaseSavedPlaybackTime;
        private float _chaseLastEndedAt;
        private bool _chaseHasHistory;

        public TickPhase Phase => TickPhase.SimulationPhase;

        private sealed class TrackRuntime
        {
            public MusicTrackConfig Config;
            public AudioSource Source;
            public bool IsRequested;
            public float CurrentVolume;
        }

        // lifecycle

        public void Initialize()
        {
            _eventBus = ServiceLocator.Get<EventBus>();

            foreach (var config in _tracks)
            {
                if (string.IsNullOrEmpty(config.Key) || config.Clip == null)
                {
                    Debug.LogWarning($"[MusicService] Skipping invalid track config: '{config.Key}'");
                    continue;
                }

                if (_runtimeTracks.ContainsKey(config.Key))
                {
                    Debug.LogWarning($"[MusicService] Duplicate track key: '{config.Key}'");
                    continue;
                }

                var sourceGo = new GameObject($"Track_{config.Key}");
                sourceGo.transform.SetParent(transform, false);

                var source = sourceGo.AddComponent<AudioSource>();
                source.clip = config.Clip;
                source.loop = config.Loop;
                source.playOnAwake = false;
                source.volume = 0f;

                _runtimeTracks[config.Key] = new TrackRuntime
                {
                    Config = config,
                    Source = source,
                    IsRequested = false,
                    CurrentVolume = 0f
                };
            }

            _eventBus.Register<ChaseStartedEvent>(OnChaseStart);
            _eventBus.Register<ChaseEndedEvent>(OnChaseEnd);
            _eventBus.Register<ShelterEnterEvent>(OnShelterEnter);
            _eventBus.Register<ShelterLeaveEvent>(OnShelterLeave);

            ServiceLocator.Get<TickSystem>().Register(this);

            // TODO: Move into separate trigger script / make unique music loader for each scene
            
            PlayTrack("Ambient");
        }

        public void Shutdown()
        {
            if (_eventBus != null)
            {
                _eventBus.Unregister<ChaseStartedEvent>(OnChaseStart);
                _eventBus.Unregister<ChaseEndedEvent>(OnChaseEnd);
                _eventBus.Unregister<ShelterEnterEvent>(OnShelterEnter);
                _eventBus.Unregister<ShelterLeaveEvent>(OnShelterLeave);
            }

            ServiceLocator.Get<TickSystem>()?.Unregister(this);
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void Tick(float deltaTime)
        {
            int maxActivePriority = int.MinValue;

            foreach (var track in _runtimeTracks.Values)
            {
                if (track.IsRequested && track.Config.Priority > maxActivePriority)
                    maxActivePriority = track.Config.Priority;
            }

            foreach (var track in _runtimeTracks.Values)
            {
                float targetVolume;

                if (!track.IsRequested)
                {
                    targetVolume = 0f;
                }
                else if (track.Config.Priority >= maxActivePriority)
                {
                    targetVolume = track.Config.MaxVolume; // на переднем плане
                }
                else
                {
                    targetVolume = track.Config.DuckedVolume; // притушен, но играет
                }

                bool isRising = targetVolume > track.CurrentVolume;
                float duration = isRising ? track.Config.FadeInDuration : track.Config.FadeOutDuration;
                float maxDelta = duration > 0f ? deltaTime / duration : float.MaxValue;

                track.CurrentVolume = Mathf.MoveTowards(track.CurrentVolume, targetVolume, maxDelta);
                track.Source.volume = track.CurrentVolume;

                if (!track.IsRequested && track.CurrentVolume <= 0f && track.Source.isPlaying)
                {
                    track.Source.Pause();
                }
            }
        }

        // generic API

        private void PlayTrack(string key, float startTime = 0f)
        {
            if (!_runtimeTracks.TryGetValue(key, out var track))
            {
                Debug.LogWarning($"[MusicService] Track '{key}' is not configured");
                return;
            }

            float clampedTime = track.Config.Loop && track.Source.clip != null
                ? startTime % track.Source.clip.length
                : startTime;

            track.Source.time = Mathf.Clamp(clampedTime, 0f, Mathf.Max(track.Source.clip.length - 0.01f, 0f));
            track.Source.UnPause();

            if (!track.Source.isPlaying)
                track.Source.Play();

            track.IsRequested = true;
        }

        private void StopTrack(string key)
        {
            if (_runtimeTracks.TryGetValue(key, out var track))
                track.IsRequested = false;
        }

        // Chase — с "продолжением с той же фазы" при быстром повторе

        private void OnChaseStart(ChaseStartedEvent e)
        {
            float resumeTime = 0f;

            if (_chaseHasHistory && _runtimeTracks.TryGetValue(ChaseTrackKey, out var track) && track.Source.clip != null)
            {
                float elapsedSinceEnd = Time.time - _chaseLastEndedAt;

                resumeTime = elapsedSinceEnd < ChaseResumeSilenceThreshold
                    ? (_chaseSavedPlaybackTime + elapsedSinceEnd) % track.Source.clip.length
                    : 0f;
            }

            PlayTrack(ChaseTrackKey, resumeTime);
        }

        private void OnChaseEnd(ChaseEndedEvent e)
        {
            if (_runtimeTracks.TryGetValue(ChaseTrackKey, out var track))
                _chaseSavedPlaybackTime = track.Source.time;

            _chaseLastEndedAt = Time.time;
            _chaseHasHistory = true;

            StopTrack(ChaseTrackKey);
        }

        // Shelter — простой луп поверх всего

        private void OnShelterEnter(ShelterEnterEvent e)
        {
            PlayTrack(ShelterTrackKey);
        }

        private void OnShelterLeave(ShelterLeaveEvent e)
        {
            StopTrack(ShelterTrackKey);
        }
    }
}