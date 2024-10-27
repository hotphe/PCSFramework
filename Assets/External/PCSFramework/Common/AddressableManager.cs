using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;

public static class AddressableManager
{
    // 캐시를 위한 딕셔너리
    private static readonly Dictionary<string, AsyncOperationHandle> _handleCache = new Dictionary<string, AsyncOperationHandle>();
    private static readonly Dictionary<string, object> _assetCache = new Dictionary<string, object>();
    private static readonly Dictionary<string, HashSet<string>> _labelToAssetsMap = new Dictionary<string, HashSet<string>>();

    // 모든 Addressable 에셋의 dependency 다운로드
    public static async UniTask<bool> DownloadAllDependencies(IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 모든 카탈로그의 키 목록 가져오기
            var locationHandle = Addressables.LoadResourceLocationsAsync("");
            await locationHandle.ToUniTask(cancellationToken: cancellationToken);

            if (locationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load resource locations");
                return false;
            }

            var locations = locationHandle.Result;
            int totalLocations = locations.Count;
            int completedLocations = 0;

            // 각 키에 대한 dependency 다운로드
            foreach (IResourceLocation location in locations)
            {
                if (cancellationToken.IsCancellationRequested) return false;

                await DownloadDependencies(location.PrimaryKey, cancellationToken);

                completedLocations++;
                progress?.Report((float)completedLocations / totalLocations);
            }

            Addressables.Release(locationHandle);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download all dependencies: {e.Message}");
            return false;
        }
    }

    // 특정 라벨의 모든 에셋 dependency 다운로드
    public static async UniTask<bool> DownloadLabelDependencies(string label, IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(label);
            await locationHandle.ToUniTask(cancellationToken: cancellationToken);

            if (locationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load resource locations for label: {label}");
                return false;
            }

            var locations = locationHandle.Result;
            int totalLocations = locations.Count;
            int completedLocations = 0;

            foreach (IResourceLocation location in locations)
            {
                if (cancellationToken.IsCancellationRequested) return false;

                await DownloadDependencies(location.PrimaryKey, cancellationToken);

                completedLocations++;
                progress?.Report((float)completedLocations / totalLocations);
            }

            Addressables.Release(locationHandle);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download label dependencies: {e.Message}");
            return false;
        }
    }
    // 의존성 다운로드
    public static async UniTask<bool> DownloadDependencies(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var downloadSize = await Addressables.GetDownloadSizeAsync(key);

            if (downloadSize > 0)
            {
                Debug.Log($"Downloading dependencies for {key}: {downloadSize} bytes");
                var downloadHandle = Addressables.DownloadDependenciesAsync(key);
                await downloadHandle.ToUniTask(cancellationToken: cancellationToken);
                Addressables.Release(downloadHandle);
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download dependencies for {key}: {e.Message}");
            return false;
        }
    }

    // 단일 에셋 로드
    public static async UniTask<T> LoadAssetAsync<T>(string key, bool canRelease, CancellationToken cancellationToken = default)
    {
        try
        {
            // 캐시 확인
            if (_assetCache.TryGetValue(key, out object cachedAsset))
            {
                if (cachedAsset is T typedAsset)
                {
                    return typedAsset;
                }
            }

            // 새로 로드
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // canRelease가 true일 때만 handle을 캐시에 저장
                if (canRelease)
                {
                    _handleCache[key] = handle;
                }
                else if (!handle.IsValid())
                {
                    // canRelease가 false인 경우, 결과만 저장하고 handle은 바로 해제
                    Addressables.Release(handle);
                }

                _assetCache[key] = handle.Result;
                return handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load asset {key}");
                return default;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load asset {key}: {e.Message}");
            return default;
        }
    }

    // 키 리스트로 여러 에셋 로드
    public static async UniTask<IList<T>> LoadAssetsAsync<T>(IList<string> keys, bool canRelease, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = new List<UniTask<T>>();
            foreach (var key in keys)
            {
                tasks.Add(LoadAssetAsync<T>(key, canRelease, cancellationToken));
            }

            return await UniTask.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load assets: {e.Message}");
            return null;
        }
    }

    // 라벨로 여러 에셋 로드
    public static async UniTask<IList<T>> LoadAssetsByLabel<T>(string label, bool canRelease = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // 라벨에 대한 캐시된 에셋 확인
            if (_labelToAssetsMap.TryGetValue(label, out HashSet<string> assetKeys))
            {
                List<T> cachedAssets = new List<T>();
                bool allCached = true;

                foreach (string key in assetKeys)
                {
                    if (_assetCache.TryGetValue(key, out object asset) && asset is T typedAsset)
                    {
                        cachedAssets.Add(typedAsset);
                    }
                    else
                    {
                        allCached = false;
                        break;
                    }
                }

                if (allCached && cachedAssets.Count > 0)
                {
                    return cachedAssets;
                }
            }

            // 새로 로드
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // canRelease가 true일 때만 handle을 캐시에 저장
                if (canRelease)
                {
                    _handleCache[label] = handle;
                }
                else if (!handle.IsValid())
                {
                    // canRelease가 false인 경우, 결과만 저장하고 handle은 바로 해제
                    Addressables.Release(handle);
                }

                // 라벨 매핑 초기화 또는 업데이트
                if (!_labelToAssetsMap.ContainsKey(label))
                {
                    _labelToAssetsMap[label] = new HashSet<string>();
                }
                else
                {
                    _labelToAssetsMap[label].Clear();
                }

                // 결과 캐싱 및 매핑
                List<T> resultList = new List<T>();
                foreach (T asset in handle.Result)
                {
                    string assetKey = ((UnityEngine.Object)(object)asset).name;
                    _assetCache[assetKey] = asset;
                    _labelToAssetsMap[label].Add(assetKey);
                    resultList.Add(asset);
                }

                return resultList;
            }
            else
            {
                Debug.LogError($"Failed to load assets with label: {label}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load assets with label {label}: {e.Message}");
            return null;
        }
    }

    // 특정 키의 에셋 해제
    public static void ReleaseAsset(string key)
    {
        if (_handleCache.TryGetValue(key, out AsyncOperationHandle handle))
        {
            if (handle.IsValid())
            {
                try
                {
                    Addressables.Release(handle);
                    _handleCache.Remove(key);
                    _assetCache.Remove(key);

                    // labelToAssetsMap에서도 제거
                    foreach (var assetSet in _labelToAssetsMap.Values)
                    {
                        assetSet.Remove(key);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to release asset {key}: {e.Message}");
                }
            }
        }
    }

    // 특정 라벨의 모든 에셋 해제
    public static void ReleaseLabel(string label)
    {
        if (_handleCache.TryGetValue(label, out AsyncOperationHandle handle))
        {
            if (handle.IsValid())
            {
                try
                {
                    Addressables.Release(handle);
                    _handleCache.Remove(label);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to release label {label}: {e.Message}");
                }
            }
        }

        // 라벨에 속한 개별 에셋들의 캐시 제거
        if (_labelToAssetsMap.TryGetValue(label, out HashSet<string> assetKeys))
        {
            foreach (string key in assetKeys.ToArray())
            {
                if (_handleCache.ContainsKey(key))
                {
                    ReleaseAsset(key);
                }
                _assetCache.Remove(key);
            }
            _labelToAssetsMap.Remove(label);
        }
    }

    // 특정 타입의 모든 에셋 해제
    public static void ReleaseAssetsByType<T>()
    {
        var keysToRemove = new List<string>();

        foreach (var kvp in _assetCache)
        {
            if (kvp.Value is T && _handleCache.ContainsKey(kvp.Key))
            {
                if(_handleCache[kvp.Key].IsValid())
                {
                    try
                    {
                        Addressables.Release(_handleCache[kvp.Key]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to release asset {kvp.Key}: {e.Message}");
                    }
                }

                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _handleCache.Remove(key);
            _assetCache.Remove(key);
        }
    }
    // 모든 에셋 해제
    public static void ReleaseAllAssets()
    {
        foreach (var handle in _handleCache.Values)
        {
            if (handle.IsValid())
            {
                try
                {
                    Addressables.Release(handle);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to release handle: {e.Message}");
                }
            }
        }

        _handleCache.Clear();
        _assetCache.Clear();
        _labelToAssetsMap.Clear();
    }
}