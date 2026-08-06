using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KONTUR___Simulation._Scripts.SceneManagement
{
    public sealed class SceneService : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string _loadingSceneName = "LoadScene";
        [SerializeField] private string _firstLevelSceneName = "Laboratory";

        private Coroutine _loadingRoutine;

        public static SceneService Instance { get; private set; }

        public SceneState State { get; } = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadScene(_firstLevelSceneName);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
        {
            if (_loadingRoutine != null)
                StopCoroutine(_loadingRoutine);

            _loadingRoutine = StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(_loadingSceneName);

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            float timer = 0f;

            while (!operation.isDone)
            {
                timer += Time.unscaledDeltaTime;

                if (operation.progress >= 0.9f && timer >= 2f)
                    operation.allowSceneActivation = true;

                yield return null;
            }

            State.Clear();
            Debug.Log("[SceneService] State cleared after scene load");

            _loadingRoutine = null;
        }
    }
}