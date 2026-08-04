using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KONTUR___Simulation._Scripts.SceneManagement
{
    public sealed class SceneService : MonoBehaviour
    {
        [SerializeField] private string _loadSceneName;
        [SerializeField] private string _firstLevelSceneName;

        private Coroutine _loadSceneRoutine;
        
        public static SceneService Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadScene(_firstLevelSceneName);
                return;
            }
            
            Destroy(gameObject);
        }

        public void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
        {
            if (_loadSceneRoutine != null)
                StopCoroutine(_loadSceneRoutine);
            
            _loadSceneRoutine = StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            var loadSceneAsyncOp = SceneManager.LoadSceneAsync(_loadSceneName);
            while(!loadSceneAsyncOp.isDone)
                yield return null;
            
            var sceneAsyncOp = SceneManager.LoadSceneAsync(sceneName);
            sceneAsyncOp.allowSceneActivation = false;
            
            while(!sceneAsyncOp.isDone)
                yield return null;
            
            sceneAsyncOp.allowSceneActivation = true;
        }
    }
}