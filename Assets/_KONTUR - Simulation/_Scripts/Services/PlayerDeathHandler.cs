using System.Collections;
using _KONTUR___Simulation._Scripts;
using _KONTUR___Simulation._Scripts.SceneManagement;
using KofeyekToolkit.Events;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.Services
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PlayerDeathHandler : MonoBehaviour, IService
    {
        [SerializeField] private AudioClip _deathStingClip;
        [SerializeField] private float _initialDelay = 0.2f;
        [SerializeField] private float _stingDelay = 0.8f;

        [Header("UI")]
        [SerializeField] private GameObject _blackScreenPanel;

        private EventBus _eventBus;
        private AudioSource _audioSource;
        private Coroutine _deathSequenceRoutine;

        public void Initialize()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            _eventBus.Register<PlayerDiedEvent>(OnPlayerDied);

            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.ignoreListenerPause = true;

            if (_blackScreenPanel != null)
                _blackScreenPanel.SetActive(false);
        }

        public void Shutdown()
        {
            if (_eventBus != null)
            {
                _eventBus.Unregister<PlayerDiedEvent>(OnPlayerDied);
            }

            if (_deathSequenceRoutine != null)
            {
                StopCoroutine(_deathSequenceRoutine);
                _deathSequenceRoutine = null;
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void OnPlayerDied(PlayerDiedEvent ev)
        {
            if (_deathSequenceRoutine != null)
                return;

            if (SceneService.Instance != null)
            {
                SceneService.Instance.State.Death = new DeathPayload(ev.DiedFrom, null);
            }

            _deathSequenceRoutine = StartCoroutine(PlayDeathSequence());
        }

        private IEnumerator PlayDeathSequence()
        {
            AudioListener.pause = true;

            if (_deathStingClip != null)
            {
                _audioSource.PlayOneShot(_deathStingClip);
            }
            
            yield return new WaitForSecondsRealtime(_initialDelay);

            if (_blackScreenPanel != null)
                _blackScreenPanel.SetActive(true);

            float stingDuration = _deathStingClip != null ? _deathStingClip.length : 0f;
            float totalWait = Mathf.Max(_stingDelay, stingDuration + 0.2f);
            yield return new WaitForSecondsRealtime(totalWait);

            AudioListener.pause = false;

            if (SceneService.Instance != null)
            {
                SceneService.Instance.ReloadScene();
            }

            _deathSequenceRoutine = null;
        }
    }
}