using System;
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
        // 캐시를 위한 딕셔너리
        // Key: Addressable Key (에셋 주소 또는 레이블)
        private static readonly Dictionary<string, AsyncOperationHandle> _handleCache = new Dictionary<string, AsyncOperationHandle>();
        // Key: Addressable Key (에셋 주소)
        private static readonly Dictionary<string, object> _assetCache = new Dictionary<string, object>();
        // Key: Label, Value: 해당 레이블에 속한 에셋들의 Addressable Key HashSet
        private static readonly Dictionary<string, HashSet<string>> _labelToAssetKeysMap = new Dictionary<string, HashSet<string>>();

        // 특정 키(들) 또는 레이블의 모든 의존성 다운로드 (내부 헬퍼)
        private static async UniTask<bool> DownloadDependenciesInternal(object keyOrLabel, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var sizeHandle = Addressables.GetDownloadSizeAsync(keyOrLabel);
                long downloadSize = await sizeHandle.ToUniTask(cancellationToken: cancellationToken);
                Addressables.Release(sizeHandle); // GetDownloadSizeAsync 핸들도 해제 필요

                if (cancellationToken.IsCancellationRequested) return false;

                if (downloadSize > 0)
                {
                    Debug.Log($"Downloading dependencies for {keyOrLabel}: {downloadSize} bytes");
                    var downloadHandle = Addressables.DownloadDependenciesAsync(keyOrLabel);
                    await downloadHandle.ToUniTask(progress: progress, cancellationToken: cancellationToken);
                    Addressables.Release(downloadHandle); // DownloadDependenciesAsync 핸들도 해제
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download dependencies for {keyOrLabel}: {e.Message}");
                return false;
            }
        }

        // 특정 키의 의존성 다운로드
        public static async UniTask<bool> DownloadDependenciesForKey(string key, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadDependenciesInternal(key, progress, cancellationToken);
        }

        // 특정 라벨의 모든 에셋 의존성 다운로드
        public static async UniTask<bool> DownloadDependenciesForLabel(string label, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadDependenciesInternal(label, progress, cancellationToken);
        }

        // 모든 등록된 Addressable 에셋의 의존성 다운로드
        public static async UniTask<bool> DownloadAllDependencies(IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // 모든 Primary Key를 가져옵니다.
                var locationsHandle = Addressables.LoadResourceLocationsAsync((IEnumerable<object>)null, Addressables.MergeMode.Union, typeof(object));
                IList<IResourceLocation> locations = await locationsHandle.ToUniTask(cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    Addressables.Release(locationsHandle);
                    return false;
                }

                // 중복을 제거한 모든 Primary Key 리스트 생성
                var allKeys = locations.Select(loc => loc.PrimaryKey).Distinct().ToList();
                Addressables.Release(locationsHandle);

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
        }


        // 단일 에셋 로드
        public static async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Addressable key cannot be null or empty.");
                return null;
            }

            // 1. 에셋 캐시 확인
            if (_assetCache.TryGetValue(key, out object cachedAsset))
            {
                if (cachedAsset is T typedAsset)
                {
                    return typedAsset;
                }
                else
                {
                    // 캐시에 있지만 타입이 다른 경우 (이론적으로는 같은 키에 다른 타입 에셋이 있으면 안됨)
                    Debug.LogError($"Asset for key '{key}' found in cache but is of type '{cachedAsset.GetType()}', expected '{typeof(T)}'.");
                    return null;
                }
            }

            // 2. 핸들 캐시 확인 (이미 로딩 중이거나 로드된 핸들 재사용 방지 목적은 아님 - Addressables가 내부적으로 처리)
            //    여기서 핸들 캐시는 주로 Release를 위해 관리. 만약 같은 키로 동시에 여러번 LoadAssetAsync가 호출되면
            //    Addressables 내부에서 핸들링 해주므로, 중복 로드는 발생하지 않음.
            //    하지만 _handleCache에 이미 있다면 Release하지 않고 반환하는 로직은 불필요.
            //    _handleCache는 성공적으로 로드된 핸들만 저장.

            AsyncOperationHandle<T> handle;
            try
            {
                // 3. 새로 로드
                handle = Addressables.LoadAssetAsync<T>(key);
                T result = await handle.ToUniTask(cancellationToken: cancellationToken);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _assetCache[key] = result; // 에셋 캐시에 결과 저장
                    _handleCache[key] = handle; // 핸들 캐시에 핸들 저장 (나중에 해제하기 위함)
                    return result;
                }
                else
                {
                    // 로드 실패 시 핸들 즉시 해제 (참조 카운트 문제 방지)
                    if (handle.IsValid()) Addressables.Release(handle);
                    Debug.LogError($"Failed to load asset with key: {key}. Status: {handle.Status}, Error: {handle.OperationException}");
                    return null;
                }
            }
            catch (Exception e) // ToUniTask 또는 LoadAssetAsync 자체에서 예외 발생 가능
            {
                Debug.LogError($"Exception while loading asset {key}: {e.Message}");
                return null;
            }
        }

        // 키 리스트로 여러 에셋 로드
        public static async UniTask<IList<T>> LoadAssetsAsync<T>(IList<string> keys, CancellationToken cancellationToken = default) where T : class
        {
            if (keys == null || keys.Count == 0)
            {
                return new List<T>();
            }

            var tasks = new List<UniTask<T>>();
            foreach (var key in keys)
            {
                tasks.Add(LoadAssetAsync<T>(key, cancellationToken));
            }

            try
            {
                T[] results = await UniTask.WhenAll(tasks);
                // WhenAll은 예외 발생 시 집계 예외를 던지므로, 개별 null 체크도 필요할 수 있음
                return results.Where(r => r != null).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load assets for multiple keys: {e.Message}");
                return new List<T>(); // 또는 null 반환
            }
        }

        // 라벨로 여러 에셋 로드
        public static async UniTask<IList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogError("Addressable label cannot be null or empty.");
                return new List<T>();
            }

            var loadedAssets = new List<T>();
            HashSet<string> assetKeysForLabel;

            // 1. 레이블에 대한 에셋 키 목록 가져오기 (또는 캐시된 정보 사용)
            if (!_labelToAssetKeysMap.TryGetValue(label, out assetKeysForLabel))
            {
                var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
                IList<IResourceLocation> locations = await locationsHandle.ToUniTask(cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    Addressables.Release(locationsHandle);
                    return new List<T>();
                }

                assetKeysForLabel = new HashSet<string>();
                if (locations != null)
                {
                    foreach (var loc in locations)
                    {
                        assetKeysForLabel.Add(loc.PrimaryKey);
                    }
                }
                _labelToAssetKeysMap[label] = assetKeysForLabel; // 맵에 저장
                Addressables.Release(locationsHandle); // 로케이션 핸들 해제
            }
            
            if (assetKeysForLabel.Count == 0)
            {
                 Debug.LogWarning($"No assets found for label: {label} and type: {typeof(T)}");
                 return loadedAssets; // 빈 리스트 반환
            }

            // 2. 각 키에 대해 LoadAssetAsync 호출
            var tasks = new List<UniTask<T>>();
            foreach (var key in assetKeysForLabel)
            {
                tasks.Add(LoadAssetAsync<T>(key, cancellationToken));
            }

            try
            {
                T[] results = await UniTask.WhenAll(tasks);
                loadedAssets.AddRange(results.Where(r => r != null));
                return loadedAssets;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load assets for label {label}: {e.Message}");
                return new List<T>(); // 또는 null 반환
            }
        }

        // 특정 키의 에셋 해제
        public static void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_handleCache.TryGetValue(key, out AsyncOperationHandle handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _handleCache.Remove(key);
            }
            _assetCache.Remove(key);

            foreach (var assetKeySet in _labelToAssetKeysMap.Values)
            {
                assetKeySet.Remove(key);
            }
        }

        // 특정 라벨의 모든 에셋 해제
        public static void ReleaseAssetsByLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return;

            if (_labelToAssetKeysMap.TryGetValue(label, out HashSet<string> assetKeys))
            {
                // ToArray()로 복사본 사용: ReleaseAsset 내부에서 assetKeys Set이 수정될 수 있기 때문
                foreach (string key in assetKeys.ToArray())
                {
                    ReleaseAsset(key);
                }
                // _labelToAssetKeysMap에서 해당 레이블 자체를 제거
                _labelToAssetKeysMap.Remove(label);
            }
        }

        // 특정 타입의 모든 에셋 해제 (주의: 비용이 클 수 있음)
        public static void ReleaseAssetsByType<T>() where T : class
        {
            var keysToRemoveFromAssetCache = new List<string>();
            foreach (var kvp in _assetCache)
            {
                if (kvp.Value is T)
                {
                    keysToRemoveFromAssetCache.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemoveFromAssetCache)
            {
                ReleaseAsset(key); // ReleaseAsset을 통해 핸들 및 모든 캐시에서 제거
            }
        }

        // 모든 캐시된 에셋 해제
        public static void ReleaseAllAssets()
        {
            foreach (var key in _handleCache.Keys.ToList())
            {
                 if (_handleCache.TryGetValue(key, out AsyncOperationHandle handle) && handle.IsValid())
                 {
                    Addressables.Release(handle);
                 }
            }

            _handleCache.Clear();
            _assetCache.Clear();
            _labelToAssetKeysMap.Clear();
            Debug.Log("All cached addressable assets released.");
        }
    }
}
