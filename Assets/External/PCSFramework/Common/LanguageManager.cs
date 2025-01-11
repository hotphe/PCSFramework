using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;
using PCS.SaveData;
#if PCS_Addressables
using PCS.Addressable;
#endif

namespace PCS.Common
{
    public static class LanguageManager
    {
        //index, header, value
        private static Dictionary<string,Dictionary<string, string>> languages;

        private const string LANGUAGE = "Language_";
        private const string FILE_NAME = "StringTable";

        public static async UniTask InitializeAsync()
        {
            await LoadLanguageAsync(OptionSaveData.Instance.Language.ToString());
        }

        public static async UniTask LoadLanguageAsync(string country)
        {
            //언어별 서로 다른 파일 사용시
            //string fileName = string.Concat(LANGUAGE, country);
            string fileName = FILE_NAME;
            TextAsset asset;
            try
            {
#if PCS_Addressable
                asset = await AddressableManager.LoadAssetAsync<TextAsset>(fileName, false);
#else
                asset = (TextAsset)await Resources.LoadAsync<TextAsset>(fileName);
#endif
            } catch (Exception e)
            {
                Debug.LogError($"Failed to load asset : {country} , Error : {e.Message}");
                return;
            }

            languages = CSVReader.ReadAll(asset);

            Debug.Log("Load Language Complete");
        }
        
        public static string GetLanguage(string value) 
        {
            if(languages.TryGetValue(value, out var dict))
            {
                if (dict.TryGetValue(OptionSaveData.Instance.Language.ToString(), out var data))
                    return data;
            }
            Debug.Log($"There is no key : {value}");
            return string.Empty;
        }
    }
}
