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
        /// Ŭ���� ��ü�� �ҷ���. 
        /// Ŭ������ static���� _instance = Load() �������� ����ϹǷ� �ʿ� ������ �� �޼ҵ尡 static.
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
        /// Ŭ������ �����ϰ� Save�� �ϹǷ� non static
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
        /// ����� ����� ��� Ŭ������ ������ �ʿ���� �� �����Ƿ� static.
        /// </summary>
        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SaveDataKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// ���̺� �����͸� �������� �ǵ���.
        /// </summary>
        /// <returns></returns>
        public static T Revert()
        {
            _instance = null;
            return Instance;
        }
    }
}
