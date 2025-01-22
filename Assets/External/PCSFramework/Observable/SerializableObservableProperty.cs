using System;
using UnityEngine;
using System.Collections.Generic;
namespace PCS.Observable
{
    [Serializable]
    public class SerializableObservableProperty<T> : ObservableProperty<T> , ISerializationCallbackReceiver
    {
        [SerializeField] T _serializedValue;
        // 초기값 설정 여부를 추적하기 위한 필드

        public SerializableObservableProperty() : base(default) { }
        public SerializableObservableProperty(T value) : base(value, EqualityComparer<T>.Default)
        {
            _serializedValue = value;
        }
        // Value로 변경 시 serialize 에러 발생
        public void OnAfterDeserialize()
        {
            _value = _serializedValue;
        }

        public void OnBeforeSerialize()
        {
        }

        // ForceChange는 Drawer에서 호출되며, OnAfterDeserialize 이후 호출됩니다.
        private void ForceChange()
        {
            OnValueChanged(_serializedValue);
        }

        protected override void OnValueChanged(T vlaue)
        {
            this._serializedValue = _value;
            base.OnValueChanged(vlaue);
        }
    }
}