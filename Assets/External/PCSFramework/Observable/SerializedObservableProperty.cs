using System;
using UnityEngine;
using System.Collections.Generic;
namespace PCS.Observable
{
    [Serializable]
    public class SerializedObservableProperty<T> : ObservableProperty<T>
    {
        [SerializeField] T value;
        // 초기값 설정 여부를 추적하기 위한 필드

        public SerializedObservableProperty() : base(default) { }
        public SerializedObservableProperty(T value) : base(value, EqualityComparer<T>.Default)
        {
        }
        
        private void ForceChange()
        {
            Value = value;
        }
    }
}