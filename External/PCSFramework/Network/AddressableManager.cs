using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace PCS.Common
{
    public static class AddressableManager
    {
        private static List<AsyncOperationHandle> releaseList = new List<AsyncOperationHandle>();

        /// <summary>
        /// Downloads the dependencies for the specified label.
        /// </summary>
        /// <param name="label">The addressable label.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        public static async UniTask<bool> DownloadDependdenciesAsync(string label)
        {
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(label, true);

            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return true;
            }
            else
            {
                Debug.LogError($"Failed to download dependencies.({label})");
                return false;
            }
        }

        /*
        /// <summary>
        /// Instantiates an addressable asset by name under a parent transform.
        /// </summary>
        /// <param name="name">The addressable asset name.</param>
        /// <param name="parent">The parent transform.</param>
        /// <returns>A UniTask representing the instantiated GameObject.</returns>
        public static async UniTask<GameObject> InstantiateAssetAsync(string name, Transform parent = null)
        {
            Debug.LogWarning("InstantiateAsync is not recommended. Use LoadAssysAsync and Instantiate intead.");
            return await Addressables.InstantiateAsync(name, parent).Task.AsUniTask();
        }
        */

        /// <summary>
        /// Loads an addressable asset into memory without instantiating it.
        /// </summary>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <param name="name">The asset name.</param>
        /// <param name="canRelease">Indicates whether the asset can be released.</param>
        /// <returns>A UniTask representing the loaded asset.</returns>
        public static async UniTask<T> LoadAssetAsync<T>(string name, bool canRelease)
        {  
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(name);

            await handle.ToUniTask();

            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (canRelease && !releaseList.Any(h => h.Equals(handle)))
                    releaseList.Add(handle);

                return handle.Result;
            }else
            {
                Debug.LogError("Load Asset Failed.");
                return default(T);
            }
        }

        /// <summary>
        /// Loads addressable assets into memory without instantiating them.
        /// </summary>
        /// <typeparam name="T">The type of the assets.</typeparam>
        /// <param name="label">The addressable label.</param>
        /// <param name="canRelease">Indicates whether the assets can be released.</param>
        /// <returns>A UniTask representing the list of loaded assets.</returns>
        public static async UniTask<List<T>> LoadAssetsAsync<T>(string label, bool canRelease)
        {
            
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);

            await handle.ToUniTask();

            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (canRelease && !releaseList.Any(h => h.Equals(handle)))
                    releaseList.Add(handle);
                return handle.Result.ToList();
            }else
            {
                Debug.LogError("Load Asset Failed.");
                return new List<T>();
            }
        }

        /// <summary>
        /// Releases a specific addressable asset by name.
        /// </summary>
        /// <param name="_name">The name of the asset to release.</param>
        public static void ReleaseAsset(string _name)
        {
            var handle = FindHandleByName(_name);
            if (handle.HasValue)
                Release(handle.Value);
            else
                Debug.LogWarning($"Failed to release Asset. No asset found with name : {_name}");
        }

        /// <summary>
        /// Finds the AsyncOperationHandle for a given asset name.
        /// </summary>
        /// <param name="assetName">The name of the asset.</param>
        /// <returns>The AsyncOperationHandle if found, otherwise null.</returns>
        private static AsyncOperationHandle? FindHandleByName(string assetName)
        {
            foreach (var handle in releaseList)
            {
                if (handle.Result != null && handle.Result is UnityEngine.Object obj && obj.name == assetName)
                {
                    return handle;
                }
            }
            return null;
        }

        /// <summary>
        /// Release specific addressable asset.
        /// </summary>
        /// <param name="handle">The handle to release.</param>
        private static void Release(AsyncOperationHandle handle)
        {
            try
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    releaseList.Remove(handle);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to release asset : {ex.Message}");
            }
        }

        /// <summary>
        /// Releas all addressable assets.
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var handle in releaseList.ToList())
            {
                try
                {
                    if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Addressables.Release(handle);
                        releaseList.Remove(handle);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to release asset : {ex.Message}");
                }
            }
        }
    }
}
