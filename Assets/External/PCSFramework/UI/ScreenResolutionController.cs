using System;
using UnityEngine;
using PCS.SaveData;
using PCS.Common;

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
        public static event Action<Vector2Int> OnUpdateDeviceResolution;
        public static Vector2Int DeviceResolution => _deviceResolution ?? (_deviceResolution = GetDeviceResolution()).Value;
        public static Vector2 ReferenceScreenSize { get; private set; } = new Vector2(1080, 1920);
        public static float ReferenceScreenRatio => ReferenceScreenSize.x / ReferenceScreenSize.y;

        private const int LowResolutionHeight = 1280;
        private const int NormalResolutionHeight = 1920;
        private static Vector2Int? _deviceResolution;

        private static Vector2Int GetDeviceResolution()
        {
        #if UNITY_EDITOR
            return new Vector2Int(Screen.width, Screen.height);
        #else
            try
            {
                var width = Display.main?.systemWidth ?? 0;
                var height = Display.main?.systemHeight ?? 0;
                if (height <= 0 || width <= 0) return default;
                if (width > height) return new Vector2Int(height, width);
                return new Vector2Int(width, height);
            }catch(Exception e)
            {
                Debug.LogError(e);
                return default;
            }
                
        #endif
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
            var currentResolution = GetDeviceResolution();
            if (currentResolution == DeviceResolution) return;
            _deviceResolution = currentResolution;
            Debug.Log("Size Changed");
            OnUpdateDeviceResolution?.Invoke(DeviceResolution);
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

            if (EnvProperties.IsDebugMode) Debug.Log($"UpdateResloution : Default({width},{height} to NewResolution({newWidth},{newHeight})");

            Screen.SetResolution((int)newWidth, (int)newHeight, OptionSaveData.Instance.IsFullScreen);
        }



    }
}
