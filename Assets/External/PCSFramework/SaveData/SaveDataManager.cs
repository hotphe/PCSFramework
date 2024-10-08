using PCS.Crypto;
using PCS.Common;
using UnityEngine;

namespace PCS.SaveData
{
    public static class SaveDataManager
    {
        private static CryptoManager _cipher;
        public static CryptoManager Cipher
        {
            get
            {
                if (_cipher == null)
                {
                    _cipher = new CryptoManager(BaseConfig.Instance.SaveDataCryptoKey, BaseConfig.Instance.DefaultCryptoOption);
                }
                return _cipher;
            }
        }

        private static JsonUtilitySerializer _serializer;

        public static ISerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new JsonUtilitySerializer();
                }
                return _serializer;
            }
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}