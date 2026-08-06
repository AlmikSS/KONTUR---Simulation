using UnityEngine;

namespace Effects.Game.Level2
{
    public class Level2TVEffect : MonoBehaviour
    {
        [SerializeField] private GameObject[] _lights;
        [SerializeField] private AudioSource _audioSource;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip _turnOnClip;
        [SerializeField] private AudioClip _loopingStatic;

        public void TurnOnTV()
        {
            foreach (var lightObj in _lights)
            {
                if (lightObj != null) 
                    lightObj.SetActive(true);
            }

            if (_audioSource != null)
            {
                if (_turnOnClip != null)
                {
                    _audioSource.PlayOneShot(_turnOnClip);
                }

                if (_loopingStatic != null)
                {
                    _audioSource.clip = _loopingStatic;
                    _audioSource.loop = true;
                    _audioSource.Play();
                }
            }
        }
    }
}