using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using PCS.Common;
using PCS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace PCS.Scene
{
    public class SceneManager : MonoSingleton<SceneManager>
    {
        public event Action BeforeSceneLoaded = delegate { };
        public event Action AfterSceneLoaded = delegate { };
        public event Action SceneLoadComplete = delegate { };

        private SceneConfig _sceneConfig;
        private SceneGroup _currentSceneGroup;
        private List<string> _loadedScene = new List<string>();

        public IReadOnlyReactiveProperty<bool> IsLoading => _isLoading;
        private ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>();

        private string[] excludes = new string[] { "Scene", "Presenter", "Installer","Model" };

        private const int MIN_LOADING_TIME = 1000;
        
        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);

            _sceneConfig = await AddressableManager.LoadAssetAsync<SceneConfig>(typeof(SceneConfig).Name,false);
            
            if(_sceneConfig == null)
            {
                Debug.LogError($"Failed to load SceneConfig. Check Internet Connection or the server status.");
                return;
            }

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            _currentSceneGroup = _sceneConfig.LoadSceneGroups.FirstOrDefault(x => x.GroupName.Equals(sceneName));

            if (_currentSceneGroup == null)
            {
                Debug.LogError($"Current Scene ({sceneName}) does not in SceneConfig.");
                return;
            }

            Debug.Log("SceneManager initialized");

#if UNITY_EDITOR
            //if not starts in start scene
            if (Bootstrap.CurrentScene != Bootstrap.START_SCENE)
            {
                //Relaod Current Scene.
                try
                {
                    var presenter = FindObjectsOfType<MonoBehaviour>().OfType<IPresenterBase>().FirstOrDefault();
                    if (presenter != null)
                    {
                        var presenterType = presenter.GetType();
                        var loadSceneAsyncMethod = typeof(SceneManager).GetMethod(nameof(LoadSceneAsync)).MakeGenericMethod(presenterType);
                        await (UniTask)loadSceneAsyncMethod.Invoke(this, new object[] { new Func<IPresenterBase, UniTask>(scene => scene.InitializeAsync()) });
                        Debug.Log("SceneReloaded.");
                    }
                    else
                    {
                        Debug.LogError($"There is no PresenterBase in {sceneName} Scene.");
                    }

                }
                catch(Exception e)
                {
                    Debug.LogError($"{e}");
                }
            }
#endif
        }

        public async UniTask LoadEssentialScene()
        {
            var operationGroup = new AsyncOperationGroup();
            var handleGruop = new AsyncOperationHandleGroup();

            foreach (var scene in _sceneConfig.EssentialScene)
            {
                if (scene.State != SceneReferenceState.Unsafe && !_loadedScene.Contains(scene.Name))
                {
                    SetOperations(scene, LoadSceneMode.Additive, operationGroup, handleGruop);
                    _loadedScene.Add(scene.Name);
                }
            }

            while (!operationGroup.IsDone || !handleGruop.IsDone)
                await UniTask.Delay(10);

        }

        public async UniTask LoadSceneAsync<T>(Func<T, UniTask> initializer = null) where T : MonoBehaviour, IPresenterBase
        {
            if(_sceneConfig == null) 
            { 
                Debug.LogError("SceneManager is not initialized.");
                return;
            }

            SceneGroup group = GetGroup(typeof(T));
            if (group == null)
            {
                Debug.LogError($"There is no scene name of {GetExclude(typeof(T).Name)}");
                return;
            }

            SceneTransitionController.Show();
            _isLoading.Value = true;

            BeforeSceneLoaded.Invoke();

            var operationGroup = new AsyncOperationGroup();
            var handleGruop = new AsyncOperationHandleGroup();
            
            SceneReference activeScene = group.ActiveScene;
            List<SceneReference> additives = group.AdditiveScenes;

            //Load Active Scene
            if (activeScene.State != SceneReferenceState.Unsafe)
                SetOperations(activeScene, LoadSceneMode.Additive, operationGroup, handleGruop);

            //Load Additive Scenes
            foreach (var additive in additives)
            {
                if (additive.State != SceneReferenceState.Unsafe)
                    SetOperations(additive, LoadSceneMode.Additive, operationGroup, handleGruop);
            }

            while (!operationGroup.IsDone || !handleGruop.IsDone)
                await UniTask.Delay(100);

            await UnloadSceneAsync();

            
            if (activeScene.State != SceneReferenceState.Unsafe)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene.LoadedScene);

            AfterSceneLoaded.Invoke();

            if (initializer != null && activeScene != null)
            {
                try
                {
                    var objects = activeScene.LoadedScene.GetRootGameObjects().Select(obj => obj.GetComponent<T>());
                    await initializer(objects.First(obj => obj != null));
                }
                catch
                {
                    Debug.LogError("Initialize presenter failed.");
                }
            }
            _currentSceneGroup = group;
            await UniTask.Delay(MIN_LOADING_TIME);
            _isLoading.Value = false;
            SceneLoadComplete.Invoke();
            SceneTransitionController.Hide();
        }

        private async UniTask UnloadSceneAsync()
        {
            if (_currentSceneGroup == null)
            {
                return;
            }

            if(_currentSceneGroup.ActiveScene.State != SceneReferenceState.Unsafe)
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_currentSceneGroup.ActiveScene.Path);

            foreach (var scene in _currentSceneGroup.AdditiveScenes)
            {
                if(scene.State != SceneReferenceState.Unsafe)
                    await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.Path);
            }
        }

        private void SetOperations(SceneReference scene,LoadSceneMode mode, AsyncOperationGroup operationGroup, AsyncOperationHandleGroup handleGroup)
        {
            if (scene.State == SceneReferenceState.Regular)
            {
                var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.Path, mode);
                operationGroup.AddOperation(operation);
            }
            else if (scene.State == SceneReferenceState.Addressable)
            {
                var handle = Addressables.LoadSceneAsync(scene.Path, mode);
                handleGroup.AddHandle(handle);
            }
        }

        private SceneGroup GetGroup(Type type)
        {
            string name = GetExclude(type.Name);
            return _sceneConfig.LoadSceneGroups.FirstOrDefault(x => x.GroupName.Equals(name));
        }

        /// <summary>
        /// string에서 excludes에 포함된 문자열을 제거함.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetExclude(string str)
        {
            return excludes.Aggregate(str, (current, exclude) => current.Replace(exclude, string.Empty));
        }
    }

    public class AsyncOperationGroup
    {
        private List<AsyncOperation> _operations;

        public float Progress => _operations.Count == 0 ? 1 : _operations.Average(o => o.progress);
        public bool IsDone => _operations.All(o => o.isDone);

        public AsyncOperationGroup()
        {
            _operations = new List<AsyncOperation>();
        }
        public void AddOperation(AsyncOperation operation)
        {
            _operations.Add(operation);
        }
    }
    public class AsyncOperationHandleGroup
    {
        private List<AsyncOperationHandle<SceneInstance>> _handles;

        public float Progress => _handles.Count == 0 ? 1 : _handles.Average(h => h.PercentComplete);
        public bool IsDone => _handles.Count == 0 || _handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup() 
        {
            _handles = new List<AsyncOperationHandle<SceneInstance>>();
        }

        public void AddHandle(AsyncOperationHandle<SceneInstance> handle)
        {
            _handles.Add(handle);
        }
    }
}
