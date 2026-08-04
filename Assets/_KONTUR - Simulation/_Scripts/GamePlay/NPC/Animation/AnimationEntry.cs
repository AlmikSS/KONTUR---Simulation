using System;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    [Serializable]
    public class AnimationEntry
    {
        [Tooltip("Key string for your animation clip (kinda \"Walk\" end etc.)")]
        public string AnimationName;
        
        [Tooltip("AnimationClip")]
        public AnimationClip Clip;

        [Tooltip("AnimationSpeed")]
        public float Speed = 1;

        public bool IsValid => !string.IsNullOrEmpty(AnimationName) && Clip != null;
    }
}