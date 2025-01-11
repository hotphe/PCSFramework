using System.Collections.Generic;

namespace PCS.Common
{
    public static class DictionaryExtension
    {
        public static SerializedDictionary<TKey,TValue> ToSerializedDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            SerializedDictionary<TKey, TValue> dict = new SerializedDictionary<TKey, TValue>(); 
            foreach(var kvp in dictionary)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            return dict;
        }
    }
}
