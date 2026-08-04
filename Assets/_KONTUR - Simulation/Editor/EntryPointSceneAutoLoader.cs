using UnityEditor;
using UnityEditor.SceneManagement;

namespace _KONTUR___Simulation.Editor
{
    [InitializeOnLoad]
    public static class EntryPointSceneAutoLoader
    {
        private const string MenuPath = "PlayFromBoot/Enable";
        private const string PlayFromBootstrapKey = "PlayFromBootKey";
        private const int BootSceneIndex = 0;

        static EntryPointSceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            var result = EditorPrefs.GetBool(PlayFromBootstrapKey);
            EditorPrefs.SetBool(PlayFromBootstrapKey, !result);
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PlayFromBootstrapKey));
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;
            
            if (!EditorPrefs.GetBool(PlayFromBootstrapKey))
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }
                
            if (EditorBuildSettings.scenes.Length == 0)
                return;
                
            EditorSceneManager.playModeStartScene = AssetDatabase.
                LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[BootSceneIndex].path);
        }
    }
}