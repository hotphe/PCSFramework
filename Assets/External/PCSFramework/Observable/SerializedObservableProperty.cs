using System;
using UnityEngine;
using System.Collections.Generic;
namespace PCS.Observable
{
    [Serializable]
    public class SerializedObservableProperty<T> : ObservableProperty<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private T value;
        public SerializedObservableProperty() : base(default) { }
        public SerializedObservableProperty(T value) : base(value, EqualityComparer<T>.Default) { }

        public void OnAfterDeserialize()
        {
            if (EqualityComparer.Equals(GetValue(), value))
                return;
            Value = value;
        }

        public void OnBeforeSerialize()
        {
            value = GetValue();
        }
    }
}