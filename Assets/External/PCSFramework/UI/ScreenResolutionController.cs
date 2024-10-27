using System;
using UnityEngine;
using PCS.SaveData;
using PCS.Common;
using Cysharp.Threading.Tasks;

namespace PCS.UI
{
    public enum QualityType
    {
        Undefined = -1,
        Normal = 0,
        High = 1,
        Low = 2,
    }
    
    public static class ScreenResolutionController
    {
        public static event Action<Vector2> OnUpdateDeviceResolution;
        public static Vector2 DeviceResolution => _deviceResolution ?? (_deviceResolution = GetDeviceResolution()).Value;
        public static Vector2 ReferenceScreenSize { get; private set; } = new Vector2(1080, 1920);
        public static float ReferenceScreenRatio => ReferenceScreenSize.x / ReferenceScreenSize.y;

        private const int LowResolutionHeight = 1280;
        private const int NormalResolutionHeight = 1920;
        private static Vector2? _deviceResolution;

        private static Vector2 GetDeviceResolution()
        {
            if (OptionSaveData.Instance.Resolution != Vector2Int.zero)
            {
                return OptionSaveData.Instance.Resolution;
            }
            else
            {
#if UNITY_EDITOR
                OptionSaveData.Instance.Resolution = new Vector2Int(Screen.width, Screen.height);
                OptionSaveData.Instance.Save();
                return new Vector2(Screen.width, Screen.height);
#else
            try
            {
                var width = Display.main?.systemWidth ?? 0;
                var height = Display.main?.systemHeight ?? 0;
                if (height <= 0 || width <= 0) return default;
                //if (width > height) return new Vector2(height, width);
                OptionSaveData.Instance.Resolution = new Vector2Int(width,height);
                OptionSaveData.Instance.Save();
                return new Vector2(width, height);
            }catch(Exception e)
            {
                Debug.LogError(e);
                return default;
            }
#endif
            }
        }

        public static void Initialize()
        {
            if (DeviceResolution == default) return;

            try
            {
                var optionData = OptionSaveData.Instance;
                var qualityType = optionData.ResolutionQuality;

                if(qualityType == QualityType.Undefined)
                {
                    qualityType = DeviceResolution.y >= NormalResolutionHeight ? QualityType.Normal : QualityType.Low;
                    optionData.ResolutionQuality = qualityType;
                    optionData.Save();
                    if (EnvProperties.IsDebugMode) Debug.Log($"Initialize Resolution : {DeviceResolution}");
                }
                UpdateResolution(qualityType);
                OnUpdateDeviceResolution?.Invoke(DeviceResolution);
            }catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void CheckUpdateDeviceResolution()
        {
#if UNITY_ANDROID || UNITY_IOS
            var currentResolution = GetDeviceResolution();
            if (currentResolution == DeviceResolution) return;
            _deviceResolution = currentResolution;
            OnUpdateDeviceResolution?.Invoke(DeviceResolution);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            OnUpdateDeviceResolution?.Invoke(DeviceResolution);
#endif
        }

        public static void UpdateResolution(QualityType qType)
        {
            var resolutionHeight = NormalResolutionHeight;
            switch(qType)
            {
                case QualityType.Low:
                    resolutionHeight = LowResolutionHeight;
                    break;
            case QualityType.Normal:
            case QualityType.High:
                    resolutionHeight = NormalResolutionHeight;
                    break;
            }

            UpdateResolutionByHeight(resolutionHeight);
        }

        public static void UpdateResolution(Vector2Int resolution)
        {
            _deviceResolution = resolution;
            OptionSaveData.Instance.Resolution = resolution;
            OptionSaveData.Instance.Save();
            Screen.SetResolution(resolution.x, resolution.y, OptionSaveData.Instance.IsFullScreen);
            OnUpdateDeviceResolution?.Invoke(resolution);
        }

        /// <summary>
        /// Height 기준으로 해상도 업데이트
        /// </summary>
        /// <param name="newHeight"></param>
        private static void UpdateResolutionByHeight(float newHeight)
        {
            if (DeviceResolution == default) return;

            var width = DeviceResolution.x;
            var height = DeviceResolution.y;
            var aspectRatio = (float)width / height;
            var newWidth = aspectRatio * newHeight;

            if (EnvProperties.IsDebugMode) 
                Debug.Log($"UpdateResloution : Default({width},{height} to NewResolution({newWidth},{newHeight})");
            
            //Not work in Editor, Mobile
            Screen.SetResolution((int)newWidth, (int)newHeight, OptionSaveData.Instance.IsFullScreen);
        }



    }
}
