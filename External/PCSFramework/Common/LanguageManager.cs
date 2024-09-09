using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using PCS.SaveData;
using UnityEngine;
using System;
using Cysharp.Text;

namespace PCS.Common
{
    public static class LanguageManager
    {
        //순서대로 index, header, value
        private static Dictionary<string,Dictionary<string, string>> languages;

        private const string LANGUAGE = "Language_";

        public static async UniTask InitializeAsync()
        {
            await LoadLanguageAsync(OptionSaveData.Instance.Language.ToString());
        }

        public static async UniTask LoadLanguageAsync(string country)
        {
            string fileName = ZString.Concat(LANGUAGE, country);
            TextAsset asset;
            try
            {
                asset = await AddressableManager.LoadAssetAsync<TextAsset>(fileName, false);
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
