using System;
using System.Text;
using UnityEngine;

namespace PCS.SaveData
{
    public abstract class SaveData<T> where T : SaveData<T>, new()
    {
        private static string _saveDataKey;

        public static string SaveDataKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_saveDataKey))
                    return _saveDataKey;

                _saveDataKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(typeof(T).ToString()));
                return _saveDataKey;
            }
        }

        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }


        public bool IsExist => PlayerPrefs.HasKey(SaveDataKey);

        /// <summary>
        /// 클래스 자체를 불러옴. 
        /// 클래스의 static에서 _instance = Load() 형식으로 사용하므로 필요 데이터 및 메소드가 static.
        /// </summary>
        /// <returns></returns>
        public static T Load()
        {
            var data = PlayerPrefs.GetString(SaveDataKey, null);

            if (string.IsNullOrEmpty(data))
                return new T();

            var converted = Convert.FromBase64String(data);
            var decrypted = SaveDataManager.Cipher.Decrypt(converted);
            return SaveDataManager.Serializer.Deserialize<T>(decrypted);
        }
        /// <summary>
        /// 클래스를 생성하고 Save를 하므로 non static
        /// </summary>
        public void Save()
        {
            var serialized = SaveDataManager.Serializer.Serialize(this);
            var encrypted = SaveDataManager.Cipher.Encrypt(serialized);
            var converted = Convert.ToBase64String(encrypted);
            PlayerPrefs.SetString(SaveDataKey, converted);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 지우는 기능의 경우 클래스의 생성이 필요없을 수 있으므로 static.
        /// </summary>
        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SaveDataKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 세이브 데이터를 기존으로 되돌림.
        /// </summary>
        /// <returns></returns>
        public static T Revert()
        {
            _instance = null;
            return Instance;
        }
    }
}
