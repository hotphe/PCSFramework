using PCS.Crypto;
using System;
using UnityEngine;

namespace PCS.Common
{
    [CreateAssetMenu(fileName = "BaseConfig", menuName = "Config/BaseConfig")]
    public class BaseConfig : ScriptableObject
    {
        private const string SaveName = "BaseConfig";

        public CryptoOption DefaultCryptoOption;

        [Header("PCS.SaveData")]
        public string SaveDataCryptoKey = "RGVmZW5zZUFsb25lRGF0YQ==";
        
        
        [Header("PCS.Network")]
        public Network.HmacConfig NetworkConfig;

        private static BaseConfig instance;

        public static BaseConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    var config = Resources.Load<BaseConfig>(SaveName);
                    instance = Instantiate(config); //When using CreateInstance, data is initiallized. When using Instantiate, data is retained.
                }
                return instance;
            }
        }
    }
}