using UnityEngine;
using UnityEngine.SceneManagement;
using PCS.Sound;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
public static class Bootstrap
{
    private const string BOOT_SCENE = "Bootstrap";
    public const string START_SCENE = "Title";
    public static string CurrentScene = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Boot()
    {
#if UNITY_EDITOR
        //In the editor, when starting from a specific scene, that scene is saved. For live builds, it starts from the scene specified in SceneConfig.startScene, so it doesn't matter.
        CurrentScene = EditorSceneManager.GetActiveScene().name;
#else
        CurrentScene = START_SCENE;
#endif

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            var candiate = SceneManager.GetSceneAt(sceneIndex);
            if (candiate.name == BOOT_SCENE)
                return;
        }
        
        SceneManager.LoadScene(BOOT_SCENE, LoadSceneMode.Additive);
    }
}
