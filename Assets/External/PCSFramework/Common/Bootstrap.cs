using UnityEngine;
using Cysharp.Threading.Tasks;
using PCS.Common;
using PCS.SceneManagement;
using PCS.DI.Core;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
public static class Bootstrap
{
    public static string CurrentScene = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Boot()
    {
#if UNITY_EDITOR
        //In the editor, when starting from a specific scene, that scene is saved. For live builds, it starts from the scene specified in SceneConfig.startScene, so it doesn't matter.
        CurrentScene = EditorSceneManager.GetActiveScene().name;
#else
        CurrentScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name;
#endif
        UniTask.Create(async () => await InitializeAsync());
    }


    //Initialize here
    private static async UniTask InitializeAsync()
    {

#if PCS_SceneManagement
        await SceneManager.Instance.InitializeAsync();
#endif
#if PCS_DI
        DIBootstrapper.Boot();
#endif
        /*
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(BOOT_SCENE, LoadSceneMode.Additive);
        await AtlasManager.Instance.InitializeAsync();
        await SoundManager.Instance.InitializeAsync();
        PCS.SceneManagement.SceneManager.Instance.InitializeAsync().Forget();
        */
        await UniTask.CompletedTask;
    }
}
