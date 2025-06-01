using System;
using System.Collections.Concurrent; // ConcurrentDictionary 사용
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;

namespace PCS.Addressable
{
    public static class AddressableManager
    {
        // 캐시를 위한 ConcurrentDictionary (스레드 안전)
        private static readonly ConcurrentDictionary<string, AsyncOperationHandle> _handleCache = new ConcurrentDictionary<string, AsyncOperationHandle>();
        private static readonly ConcurrentDictionary<string, object> _assetCache = new ConcurrentDictionary<string, object>();
        // Value 인 HashSet<string> 자체는 스레드 안전하지 않으므로, 해당 Set 수정 시 lock 필요
        private static readonly ConcurrentDictionary<string, HashSet<string>> _labelToAssetKeysMap = new ConcurrentDictionary<string, HashSet<string>>();

        private static async UniTask<bool> DownloadDependenciesInternal(object keyOrLabel, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            AsyncOperationHandle<long> sizeHandle = default;
            AsyncOperationHandle downloadHandle = default;
            try
            {
                sizeHandle = Addressables.GetDownloadSizeAsync(keyOrLabel);
                long downloadSize = await sizeHandle.ToUniTask(cancellationToken: cancellationToken);
                // sizeHandle은 여기서 바로 해제해도 무방하나, finally에서 일괄 처리 가능
                // if (sizeHandle.IsValid()) Addressables.Release(sizeHandle); // 즉시 해제 옵션

                if (cancellationToken.IsCancellationRequested) return false;

                if (downloadSize > 0)
                {
                    Debug.Log($"Downloading dependencies for {keyOrLabel}: {downloadSize} bytes");
                    downloadHandle = Addressables.DownloadDependenciesAsync(keyOrLabel);
                    await downloadHandle.ToUniTask(progress: progress, cancellationToken: cancellationToken);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download dependencies for {keyOrLabel}: {e.Message} {e.StackTrace}");
                return false;
            }
            finally
            {
                if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);
                if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
            }
        }

        public static async UniTask<bool> DownloadDependenciesForKey(string key, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadDependenciesInternal(key, progress, cancellationToken);
        }

        public static async UniTask<bool> DownloadDependenciesForLabel(string label, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadDependenciesInternal(label, progress, cancellationToken);
        }

        public static async UniTask<bool> DownloadAllDependencies(IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = default;
            try
            {
                locationsHandle = Addressables.LoadResourceLocationsAsync((IEnumerable<object>)null, Addressables.MergeMode.Union, typeof(object));
                IList<IResourceLocation> locations = await locationsHandle.ToUniTask(cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested) return false;
                
                var allKeys = locations?.Select(loc => loc.PrimaryKey).Distinct().ToList() ?? new List<string>();

                if (allKeys.Count == 0)
                {
                    Debug.Log("No addressable assets found to download dependencies for.");
                    return true;
                }
                
                return await DownloadDependenciesInternal(allKeys, progress, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download all dependencies: {e.Message}");
                return false;
            }
            finally
            {
                if (locationsHandle.IsValid()) Addressables.Release(locationsHandle);
            }
        }

        public static async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Addressable key cannot be null or empty.");
                return null;
            }

            if (_assetCache.TryGetValue(key, out object cachedAsset))
            {
                if (cachedAsset is T typedAsset)
                {
                    return typedAsset;
                }
                else
                {
                    // 1. 타입 불일치 시 기존 캐시된 에셋 해제
                    Debug.LogError($"Type mismatch for key '{key}'. Cached type: '{cachedAsset?.GetType()}', Requested type: '{typeof(T)}'. Releasing cached asset.");
                    ReleaseAsset(key); // ReleaseAsset 내부에서 _assetCache와 _handleCache 모두 정리
                    return null;
                }
            }

            AsyncOperationHandle<T> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                T result = await handle.ToUniTask(cancellationToken: cancellationToken);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // AddOrUpdate 사용 시, 이미 키가 있다면 기존 핸들 처리 주의 필요.
                    // 여기서는 ReleaseAsset이 먼저 호출되어 정리되었거나, 새로운 로드이므로 단순 할당.
                    // 또는, 이미 로드된 다른 타입의 핸들이 ReleaseAsset에 의해 정리된 후이므로 괜찮음.
                    _assetCache[key] = result; 
                    
                    // 기존에 동일 키로 다른 핸들(아마도 실패했거나, 다른 타입으로 로드 시도 후 정리된 핸들)이 남아있을 수 있으므로,
                    // 덮어쓰기 전에 기존 핸들이 있다면 해제하는 것이 더 안전할 수 있으나,
                    // 현재 로직 상으로는 타입 미스매치 시 ReleaseAsset이 호출되어 이전에 생성된 핸들은 정리됨.
                    // 정상적인 경우라면, 이전에 이 키로 성공적으로 로드된 핸들이 _handleCache에 남아있지 않아야 함 (이미 _assetCache에서 반환되었으므로).
                    // 만약 _assetCache에는 없는데 _handleCache에만 남아있는 경우는 이전 로드 실패 후 정리 안된 케이스.
                    // ReleaseAsset(key)를 호출하기 전에 다른 타입의 핸들이 캐시에 남아있을 수 있으므로 정리.
                    if (_handleCache.TryGetValue(key, out var oldHandle) && oldHandle.IsValid())
                    {
                        Addressables.Release(oldHandle); // 이전 핸드 해제 (만약의 경우 대비)
                    }
                    _handleCache[key] = handle;
                    return result;
                }
                else
                {
                    Debug.LogError($"Failed to load asset with key: {key}. Status: {handle.Status}, Error: {handle.OperationException?.Message}");
                    // 실패 시 핸들 즉시 해제
                    if (handle.IsValid()) Addressables.Release(handle);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while loading asset {key}: {e.Message} {e.StackTrace}");
                if (handle.IsValid()) Addressables.Release(handle); // 예외 발생 시에도 핸들 해제
                return null;
            }
        }

        public static async UniTask<IList<T>> LoadAssetsAsync<T>(IList<string> keys, CancellationToken cancellationToken = default) where T : class
        {
            if (keys == null || keys.Count == 0)
            {
                return new List<T>();
            }

            var tasks = new List<UniTask<T>>();
            foreach (var key in keys.Distinct()) // 중복 키에 대한 중복 호출 방지
            {
                tasks.Add(LoadAssetAsync<T>(key, cancellationToken));
            }

            try
            {
                T[] results = await UniTask.WhenAll(tasks);
                return results.Where(r => r != null).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load assets for multiple keys: {e.Message}");
                return new List<T>();
            }
        }

        public static async UniTask<IList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogError("Addressable label cannot be null or empty.");
                return new List<T>();
            }

            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = default;
            HashSet<string> assetKeysForLabel;

            // 4. _labelToAssetKeysMap은 ConcurrentDictionary. GetOrAdd로 안전하게 HashSet 인스턴스 가져오기/생성.
            // HashSet 자체에 대한 수정은 lock으로 보호.
            assetKeysForLabel = _labelToAssetKeysMap.GetOrAdd(label, _ => new HashSet<string>());

            // HashSet이 비어있거나, (더 정교하게는) 마지막 업데이트 이후 시간이 꽤 흘렀다면 위치 정보 갱신.
            // 여기서는 단순하게, 비어있을 때만 로드. 이미 로드된 레이블 정보는 재사용.
            // (만약 레이블의 내용이 동적으로 자주 바뀐다면 다른 전략 필요)
            if (assetKeysForLabel.Count == 0) // 또는 특정 조건으로 갱신
            {
                try
                {
                    locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
                    IList<IResourceLocation> locations = await locationsHandle.ToUniTask(cancellationToken: cancellationToken);

                    if (cancellationToken.IsCancellationRequested) return new List<T>();

                    if (locations != null)
                    {
                        lock (assetKeysForLabel) // HashSet 수정 전 lock
                        {
                            assetKeysForLabel.Clear(); // 기존 것 비우고 새로 채움 (갱신 시)
                            foreach (var loc in locations)
                            {
                                assetKeysForLabel.Add(loc.PrimaryKey);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load resource locations for label {label}: {e.Message}");
                    return new List<T>();
                }
                finally
                {
                    if (locationsHandle.IsValid()) Addressables.Release(locationsHandle);
                }
            }
            
            if (assetKeysForLabel.Count == 0)
            {
                 Debug.LogWarning($"No asset keys found or loaded for label: {label} and type: {typeof(T)}");
                 return new List<T>();
            }
            
            // assetKeysForLabel은 이제 현 스레드에서 안전하게 사용할 수 있는 HashSet의 키 목록 (읽기 전용으로 사용).
            // ToArray를 사용하여 반복 중 수정에 안전한 복사본 생성.
            List<string> keysToLoad;
            lock(assetKeysForLabel) // 읽기 중 다른 스레드에서의 변경 방지 (또는 ToArray로 복사)
            {
                keysToLoad = assetKeysForLabel.ToList(); 
            }

            return await LoadAssetsAsync<T>(keysToLoad, cancellationToken);
        }

        public static void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_handleCache.TryRemove(key, out AsyncOperationHandle handle)) // ConcurrentDictionary.TryRemove
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _assetCache.TryRemove(key, out _); // ConcurrentDictionary.TryRemove

            foreach (var kvp in _labelToAssetKeysMap)
            {
                HashSet<string> assetKeySet = kvp.Value;
                lock (assetKeySet) // HashSet 수정 전 lock
                {
                    assetKeySet.Remove(key);
                }
            }
        }

        public static void ReleaseAssetsByLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return;

            if (_labelToAssetKeysMap.TryGetValue(label, out HashSet<string> assetKeys))
            {
                List<string> keysToRelease;
                lock (assetKeys) // HashSet 읽기 전 lock (또는 ToArray로 복사)
                {
                    keysToRelease = assetKeys.ToList();
                }

                foreach (string key in keysToRelease)
                {
                    ReleaseAsset(key);
                }
            }
             _labelToAssetKeysMap.TryRemove(label, out _);
        }

        public static void ReleaseAssetsByType<T>() where T : class
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _assetCache)
            {
                if (kvp.Value is T)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                ReleaseAsset(key);
            }
        }

        public static void ReleaseAllAssets()
        {
            foreach (var key in _handleCache.Keys.ToList()) 
            {
                 if (_handleCache.TryRemove(key, out AsyncOperationHandle handle) && handle.IsValid())
                 {
                    Addressables.Release(handle);
                 }
            }
            _assetCache.Clear();
            _labelToAssetKeysMap.Clear(); // 모든 레이블 매핑 제거
            Debug.Log("All cached addressable assets released.");
        }
    }
}
