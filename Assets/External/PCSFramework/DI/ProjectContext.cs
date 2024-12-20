using UnityEngine;
using PCS.Scene;
namespace PCS.DI
{
    public class ProjectContext : ContextBase
    {
        protected override LifecycleScope _scope => LifecycleScope.Project;
        protected override void Awake()
        {
            SceneManager.AfterSceneLoaded += OnSceneLoaded;
            base.Awake();
        }

        protected virtual void OnDestroy()
        {
            SceneManager.AfterSceneLoaded -= OnSceneLoaded;
        }
    }
}
