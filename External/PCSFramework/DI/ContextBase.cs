using PCS.Common;
using PCS.SaveData;
using PCS.Scene;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VInspector;
using System.Data.Common;

namespace PCS.DI
{
    [DefaultExecutionOrder(-9999)]
    public abstract class ContextBase : MonoBehaviour
    {
        [Tooltip("Set Resource Prefab.")]
        [SerializeField] protected List<ProviderBase> _providerPrefab = new List<ProviderBase>();
        protected List<IDependencyProvider> providers = new List<IDependencyProvider>();
        protected DIContainer _diContainer;
        
        protected virtual void Awake()
        {
            Initialize();
        }

        protected void Initialize() 
        {
            _diContainer = new DIContainer();
            providers.AddRange(GetComponents<IDependencyProvider>());
            providers.AddRange(_providerPrefab);    
            _diContainer.Regist(providers);
            Inject();
        }


        protected virtual void Inject()
        {
            var monoBehaviours = FindMonoBehaviours();
            _diContainer.Inject(monoBehaviours);
        }
        
        protected MonoBehaviour[] FindMonoBehaviours() => FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
    }
}
