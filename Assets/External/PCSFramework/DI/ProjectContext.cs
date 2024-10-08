using UnityEngine;
using PCS.Common;
using PCS.SaveData;
using PCS.Scene;
using Cysharp.Threading.Tasks;
using PCS.Sound;
namespace PCS.DI
{
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
            await AtlasManager.Instance.InitializeAsync();
            await SoundManager.Instance.InitializeAsync();

            SceneManager.Instance.AfterSceneLoaded += OnSceneLoaded;
            SceneManager.Instance.InitializeAsync().Forget();
        }
        private void OnSceneLoaded()
        {
            Inject();
        }

    }
}
