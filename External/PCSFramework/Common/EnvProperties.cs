using UnityEngine;

namespace PCS.Common
{
    public static class EnvProperties
    {
    #if DEVELOP
        public static bool IsDevelop => true;
    #else
        public static bool IsDevelop => false;
    #endif

    #if STAGING
        public static bool IsStaging => true;
    #else
        public static bool IsDStaging => false;
    #endif

    #if PRODUCTION
        public static bool IsProduction => true;
        public static bool IsDebugMode => false;
    #else
        public static bool IsProduction => false;
        public static bool IsDebugMode => true;
    #endif

        public static readonly string ProductAppBundleId = "";
        public static bool IsProductionBundleID => Application.identifier.Equals(ProductAppBundleId);
        public static bool IsAndroid => Application.platform == RuntimePlatform.Android;
        public static bool IsIOS => Application.platform == RuntimePlatform.IPhonePlayer;






    }
}
