using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using PCS.Common;
using PCS.Sound;
using PCS.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
public static class Bootstrap
{
    private const string BOOT_SCENE = "Bootstrap";
    public const string START_SCENE = "Logo";
    public static string CurrentScene = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Boot()
    {
#if UNITY_EDITOR
        //In the editor, when starting from a specific scene, that scene is saved. For live builds, it starts from the scene specified in SceneConfig.startScene, so it doesn't matter.
        CurrentScene = EditorSceneManager.GetActiveScene().name;
#else
        CurrentScene = SceneManager.GetSceneAt(0).name;
#endif
        UniTask.Create(async () => await InitializeAsync());
    }


    //각종 초기화 작업 수행
    private static async UniTask InitializeAsync()
    {
        ScreenResolutionController.Initialize();
        await SceneManager.LoadSceneAsync(BOOT_SCENE, LoadSceneMode.Additive);
        await AtlasManager.Instance.InitializeAsync();
        await SoundManager.Instance.InitializeAsync();
        PCS.Scene.SceneManager.Instance.InitializeAsync().Forget();
    }
}
