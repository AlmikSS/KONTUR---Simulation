using System;
using _KONTUR___Simulation._Scripts.SceneManagement;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay
{
    public sealed class LastLevelTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            SceneService.Instance.LoadScene("Titri");
        }
    }
}