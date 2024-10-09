using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    public List<SerializedKeyValuePair<TKey, TValue>> SerializedKvps = new();

    public void OnBeforeSerialize()
    {
        foreach (var kvp in this)
            if (SerializedKvps.FirstOrDefault(r => this.Comparer.Equals(r.Key, kvp.Key)) is SerializedKeyValuePair<TKey, TValue> serializedKvp)
            {
                serializedKvp.Value = kvp.Value;
            }
            else
            {
                SerializedKvps.Add(kvp);
            }

        SerializedKvps.RemoveAll(r => r.Key is not null && !this.ContainsKey(r.Key));

        for (int i = 0; i < SerializedKvps.Count; i++)
            SerializedKvps[i].Index = i;

    }
    public void OnAfterDeserialize()
    {
        this.Clear();

        foreach (var serializedKvp in SerializedKvps)
        {
            serializedKvp.IsKeyNull = serializedKvp.Key is null;
            serializedKvp.IsKeyRepeated = serializedKvp.Key is not null && this.ContainsKey(serializedKvp.Key);

            if (serializedKvp.IsKeyNull) continue;
            if (serializedKvp.IsKeyRepeated) continue;

            this.Add(serializedKvp.Key, serializedKvp.Value);
        }
    }

    [Serializable]
    public class SerializedKeyValuePair<TKey_, TValue_>
    {
        public TKey_ Key;
        public TValue_ Value;

        public int Index;

        public bool IsKeyRepeated;
        public bool IsKeyNull;

        public SerializedKeyValuePair(TKey_ key, TValue_ value) { this.Key = key; this.Value = value; }

        public static implicit operator SerializedKeyValuePair<TKey_, TValue_>(KeyValuePair<TKey_, TValue_> kvp) => new(kvp.Key, kvp.Value);
        public static implicit operator KeyValuePair<TKey_, TValue_>(SerializedKeyValuePair<TKey_, TValue_> kvp) => new(kvp.Key, kvp.Value);

    }
}
