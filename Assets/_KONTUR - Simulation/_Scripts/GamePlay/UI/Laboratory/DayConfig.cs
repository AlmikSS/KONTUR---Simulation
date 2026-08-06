using System;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.UI.Laboratory
{
    [CreateAssetMenu(menuName = "Configs/DayConfig")]
    public sealed class DayConfig : ScriptableObject
    {
        [SerializeField] private Replic[] _replics;
        
        public Replic[] Replics => _replics;
    }

    [Serializable]
    public sealed class Replic
    {
        public string Name;
        public string Text;
        public Sprite Image;
    }
}//1528273