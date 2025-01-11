using Eflatun.SceneReference;
using PCS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

#if PCS_Addressable
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace PCS.SceneManagement
{
    public class SceneManager : MonoSingleton<SceneManager>
    {
        public event Action BeforeSceneLoaded;
        public event Action AfterSceneLoaded;
        public event Action SceneLoadComplete;
        public bool IsLoading { get; private set; }

        private SceneConfig _sceneConfig;
        private List<string> _essentialScene = new List<string>();
        
        private Scene _currentActiveScene;
        public string CurrentActiveSceneName => _currentActiveScene.name;
        private Scene _pastActiveScene;
        public string PastActiveSceneName => _pastActiveScene.name;

        private string[] excludes = new string[] {"Presenter"};
        private const int MIN_LOADING_TIME = 1000;

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);
#if PCS_Addressable
            _sceneConfig = await AddressableManager.LoadAssetAsync<SceneConfig>(SceneConfig.FileName,false);
#else
            _sceneConfig = (SceneConfig)await Resources.LoadAsync<SceneConfig>(SceneConfig.FileName);
#endif
            if (_sceneConfig == null)
            {
                Debug.LogError($"Failed to load SceneConfig. Check Internet Connection or the server status.");
                return;
            }
            _currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            _pastActiveScene = _currentActiveScene;
            await LoadEssentialScene();

#if UNITY_EDITOR
            //if not starts in start scene
            if (Bootstrap.CurrentScene != _sceneConfig.StartSceneName)
            {
                Debug.Log("Reload Scene");

                //Relaod Current Active Scene.
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
                        Debug.LogError($"There is no PresenterBase in {CurrentActiveSceneName} Scene.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e}");
                }
            }
#endif

            Debug.Log("SceneManager initialized");

            BeforeSceneLoaded += SceneTransitionController.Show;
            SceneLoadComplete += SceneTransitionController.Hide;
        }

#if PCS_Addressable
        private async UniTask LoadEssentialScene()
        {
            var operationGroup = new AsyncOperationGroup();
            var handleGruop = new AsyncOperationHandleGroup();

            foreach (var scene in _sceneConfig.EssentialScenes)
            {
                if (scene.State != SceneReferenceState.Unsafe && !_essentialScene.Contains(scene.Name))
                {
                    SetOperations(scene, LoadSceneMode.Additive, operationGroup, handleGruop);
                    _essentialScene.Add(scene.Name);
                }
            }

            while (!operationGroup.IsDone || !handleGruop.IsDone)
                await UniTask.Yield();

        }

        public async UniTask LoadSceneAsync<T>(Func<T, UniTask> initializer = null) where T : MonoBehaviour, IPresenterBase
        {
            if (_sceneConfig == null)
            {
                Debug.LogError("SceneManager is not initialized.");
                return;
            }

            if (!HasScene(typeof(T), out var activeScene, out var additives))
            {
                Debug.LogError($"There is no scene name of {GetExclude(typeof(T).Name)}");
                return;
            }

            BeforeSceneLoaded?.Invoke();

            IsLoading = true;

            if (_sceneConfig.UseLoadingScene && _sceneConfig.LoadingScene != null)
            {
                await Addressables.LoadSceneAsync(_sceneConfig.LoadingScene.Path, LoadSceneMode.Additive);
                await UnloadSceneAsync();
            }

            //Load Active Scene
            if (activeScene.State != SceneReferenceState.Unsafe)
                await Addressables.LoadSceneAsync(activeScene.Path, LoadSceneMode.Additive);

            if (!_sceneConfig.UseLoadingScene)
                await UnloadSceneAsync();

            _currentActiveScene = activeScene.LoadedScene;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(_currentActiveScene);

            var operationGroup = new AsyncOperationGroup();
            var handleGruop = new AsyncOperationHandleGroup();

            //Load Additive Scenes
            foreach (var additive in additives)
            {
                if (additive.State != SceneReferenceState.Unsafe)
                    SetOperations(additive, LoadSceneMode.Additive, operationGroup, handleGruop);
            }

            while (!operationGroup.IsDone || !handleGruop.IsDone)
                await UniTask.Yield();

            if (_sceneConfig.UseLoadingScene && _sceneConfig.LoadingScene != null)
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_sceneConfig.LoadingScene.Path);
            }

            AfterSceneLoaded?.Invoke();

            if (initializer != null && activeScene != null)
            {
                try
                {
                    var objects = activeScene.LoadedScene.GetRootGameObjects().Select(obj => obj.GetComponent<T>());
                    await initializer(objects.First(obj => obj != null));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Initialize presenter failed. {e}");
                }
            }

            await UniTask.Delay(MIN_LOADING_TIME);
            IsLoading = false;
            SceneLoadComplete?.Invoke();
        }

        private void SetOperations(SceneReference scene, LoadSceneMode mode, AsyncOperationGroup operationGroup, AsyncOperationHandleGroup handleGroup)
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
#else
        private async UniTask LoadEssentialScene()
        {
            var operationGroup = new AsyncOperationGroup();

            foreach (var scene in _sceneConfig.EssentialScenes)
            {
                if (scene.State != SceneReferenceState.Unsafe && !_essentialScene.Contains(scene.Name))
                {
                    SetOperations(scene, LoadSceneMode.Additive, operationGroup);
                    _essentialScene.Add(scene.Name);
                }
            }

            while (!operationGroup.IsDone)
                await UniTask.Yield();
        }

        public async UniTask LoadSceneAsync<T>(Func<T, UniTask> initializer = null) where T : MonoBehaviour, IPresenterBase
        {
            if (_sceneConfig == null)
            {
                Debug.LogError("SceneManager is not initialized.");
                return;
            }

            if(!HasScene(typeof(T), out var activeScene, out var additives))
            {
                Debug.LogError($"There is no scene name of {GetExclude(typeof(T).Name)}");
                return;
            }

            BeforeSceneLoaded?.Invoke();

            IsLoading = true;

            if (_sceneConfig.UseLoadingScene && _sceneConfig.LoadingScene != null)
            {
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_sceneConfig.LoadingScene.Path, LoadSceneMode.Additive);
                await UnloadSceneAsync();
            }

            //Load Active Scene
            if (activeScene.State != SceneReferenceState.Unsafe)
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(activeScene.Path, LoadSceneMode.Additive);
            if (!_sceneConfig.UseLoadingScene)
                await UnloadSceneAsync();

            _currentActiveScene = activeScene.LoadedScene;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(_currentActiveScene);

            var operationGroup = new AsyncOperationGroup();

            //Load Additive Scenes
            foreach (var additive in additives)
            {
                if (additive.State != SceneReferenceState.Unsafe)
                    SetOperations(additive, LoadSceneMode.Additive, operationGroup);
            }

            while (!operationGroup.IsDone)
                await UniTask.Yield();

            if (_sceneConfig.UseLoadingScene && _sceneConfig.LoadingScene != null)
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_sceneConfig.LoadingScene.Path);
            }

            _pastActiveScene = _currentActiveScene;

            AfterSceneLoaded?.Invoke();

            if (initializer != null && activeScene != null)
            {
                try
                {
                    var objects = activeScene.LoadedScene.GetRootGameObjects().Select(obj => obj.GetComponent<T>());
                    await initializer(objects.First(obj => obj != null));
                }
                catch (Exception e)
                {
                    Debug.LogError($"There is no PresenterBase in {activeScene.Name} Scene. \n message:{e}");
                    
                }
            }
            
            await UniTask.Delay(MIN_LOADING_TIME);
            IsLoading = false;
            SceneLoadComplete?.Invoke();
        }

        private void SetOperations(SceneReference scene,LoadSceneMode mode, AsyncOperationGroup operationGroup)
        {
            if (scene.State == SceneReferenceState.Regular)
            {
                var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.Path, mode);
                operationGroup.AddOperation(operation);
            }
        }
#endif

        
        private async UniTask UnloadSceneAsync()
        {
            if (!HasScene(PastActiveSceneName, out var pastActiveScene, out var pastAdditives))
            {
                Debug.LogError($"There is no scene name of {PastActiveSceneName}");
                return;
            }

            //For the DIContainer in PCS.DI , additive scenes are unloaded first.
            foreach (var scene in pastAdditives)
            {
                if (!scene.LoadedScene.isLoaded)
                    continue;
                if (scene.State != SceneReferenceState.Unsafe)
                    await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.Path);
            }

            if (pastActiveScene.State != SceneReferenceState.Unsafe
                && pastActiveScene.LoadedScene == _pastActiveScene)
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_pastActiveScene);
            }
        }

        private bool HasScene(Type type, out SceneReference activeScene, out List<SceneReference> additives)
        {
            return HasScene(GetExclude(type.Name), out activeScene, out additives);
        }
        private bool HasScene(string sceneName, out SceneReference activeScene, out List<SceneReference> additives)
        {
            SceneGroup group = GetSceneGroupByName(sceneName);
            activeScene = group.ActiveScene;
            additives = group.AdditiveScenes;

            if (activeScene == null || additives == null)
                return false;
            return true;
        }

        private SceneGroup GetSceneGroupByName(string sceneName)
        {
            return _sceneConfig.Hierarchy.FirstOrDefault(x => x.ActiveSceneName.Equals(sceneName));
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


}
