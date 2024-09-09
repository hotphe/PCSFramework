using UnityEngine;
using PCS.DI;
using PCS.Common;
using PCS.SaveData;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class ProjectContext : ContextBase
{
    protected async override void Awake()
    {
        base.Awake();
        if (OptionSaveData.Instance.isFirstRun)
        {
            OptionSaveData.Instance.isFirstRun = false;
            OptionSaveData.Instance.Language = Application.systemLanguage;
            OptionSaveData.Instance.Save();
        }
        await LanguageManager.InitializeAsync();
        await AtlasManager.InitializeAsync();
        PCS.Scene.SceneManager.Instance.InitializeAsync().Forget();
        PCS.Scene.SceneManager.Instance.AfterSceneLoaded += OnSceneLoaded;

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Inject();
    }

    private void OnSceneLoaded()
    {
        Inject();
    }
}
