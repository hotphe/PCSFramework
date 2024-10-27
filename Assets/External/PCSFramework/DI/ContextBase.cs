using PCS.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.DI
{
    [DefaultExecutionOrder(-9999)]
    public abstract class ContextBase : MonoBehaviour
    {
        protected List<IDependencyProvider> providers = new List<IDependencyProvider>();
        protected DIContainer _diContainer;
        protected abstract LifecycleScope _scope { get; }
        
        protected virtual void Awake()
        {
            Initialize();
        }

        protected void Initialize() 
        {
            _diContainer = new DIContainer(_scope);
            providers.AddRange(GetComponents<IDependencyProvider>());
            _diContainer.Regist(providers);
            Inject();
        }

        protected virtual void Inject()
        {
            var monoBehaviours = FindMonoBehaviours();
            _diContainer.Inject(monoBehaviours);
        }
        
        protected MonoBehaviour[] FindMonoBehaviours() => FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);

        protected void OnSceneLoaded()
        {
            Inject();
        }
    }
}
