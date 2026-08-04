using System;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.Services
{
    [Serializable]
    public sealed class MusicTrackConfig
    {
        public string Key;
        public AudioClip Clip;
        public int Priority;
        public bool Loop = true;
        [Range(0f, 1f)] public float MaxVolume = 1f;
        [Range(0f, 1f)] public float DuckedVolume = 0.35f;
        public float FadeInDuration = 1.5f;
        public float FadeOutDuration = 1.5f;
    }
}