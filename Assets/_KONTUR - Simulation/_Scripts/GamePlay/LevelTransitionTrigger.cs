using _KONTUR___Simulation._Scripts.GamePlay.UI.Laboratory;
using _KONTUR___Simulation._Scripts.SceneManagement;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay
{
    public sealed class LevelTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private DayConfig _config;
        [SerializeField] private int _nextLevel;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SceneService.Instance.State.CurrentLevel = _nextLevel;
                SceneService.Instance.State.DayConfig = _config;
                SceneService.Instance.LoadScene("Laboratory");
            }
        }
    }
}