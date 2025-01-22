using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace PCS.Observable
{
    // 미구현
    [Serializable]
    public class SerializedObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        private List<SerializedKeyValuePair<TKey, TValue>> _prevSerializedKvps = new();

        public void OnAfterDeserialize()
        {

            if (_prevSerializedKvps == null) _prevSerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>(SerializedKvps);
            if (_prevSerializedKvps.Count != SerializedKvps.Count)
            {
                if (SerializedKvps.Count > _prevSerializedKvps.Count)
                {
                    var tempList = SerializedKvps.Where(pair => !_prevSerializedKvps.Contains(pair)).ToList();

                    SerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>(_prevSerializedKvps);

                    foreach (var pair in tempList)
                    {
                        SerializedKvps.Add(pair);
                        ObserveAdd().Notify(pair);
                    }
                    ObserveCount().Notify(SerializedKvps.Count);
                }
                else if (SerializedKvps.Count < _prevSerializedKvps.Count)
                {
                    var tempList = SerializedKvps.ToList();
                    SerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>(_prevSerializedKvps);
                    foreach (var pair in tempList)
                    {
                        if (!_prevSerializedKvps.Contains(pair))
                        {
                            SerializedKvps.Remove(pair);
                            ObserveRemove().Notify(pair);
                        }
                    }
                    ObserveCount().Notify(SerializedKvps.Count);
                }
            }
            base.OnAfterDeserialize();
        }

        public void OnBeforeSerialize()
        {
            //_prevSerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>(SerializedKvps);
            base.OnBeforeSerialize();
            _prevSerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>(SerializedKvps);
        }

    }
}