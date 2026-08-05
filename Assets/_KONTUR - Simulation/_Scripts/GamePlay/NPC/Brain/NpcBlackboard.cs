using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.NPC.Brain
{
    public sealed class NpcBlackboard
    {
        public Vector3 PlayerPosition { get; private set; }
        public bool IsPlayerInVision { get; private set; }
        public bool IsPlayerHidden { get; private set; }

        public void SetPlayerPosition(
            Vector3 playerTransform,
            bool isPlayerInVision
        ) {
            PlayerPosition = playerTransform;
            IsPlayerInVision = isPlayerInVision;
            
        }

        public void SetPlayerHidden(bool isPlayerHidden)
        {
            IsPlayerHidden = isPlayerHidden;
        }
    }
}