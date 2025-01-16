using System;
using System.Collections.Generic;
using UnityEngine;
namespace PCS.Observable
{
    [Serializable]
    public class SerializedObservableList<T> : ObservableList<T>, ISerializationCallbackReceiver
    {
        // 인스펙터의 + - 버튼으로 List 수정 시, 기본적으로 list를 아예 새로 할당합니다.
        // 이때 Subscribe된 객체가 모두 해제되므로 복사본을 사용합니다.
        private List<T> _prevItems;
        public void OnAfterDeserialize()
        {
            if (_prevItems == null) _prevItems = new List<T>(_items);
            if (_items.Count != _prevItems.Count)
            {
                if (_items.Count > _prevItems.Count)
                {
                    List<T> tempList = new List<T>(_items);
                    int newItemCount = _items.Count - _prevItems.Count;
                    _items = new List<T>(_prevItems);
                    for (int i = tempList.Count - newItemCount; i < tempList.Count; i++)
                    {
                        _items.Add(tempList[i]);
                        ObserveAdd().Nofity(new CollectionObserve<T>(index: i, newValue: _items[i]));
                    }
                    ObserveCount().Nofity(_items.Count);
                }
                else if (_items.Count < _prevItems.Count)
                {
                    int value = _prevItems.Count - _items.Count;

                    _items = new List<T>(_prevItems);
                    for (int i = _prevItems.Count - value; i < _prevItems.Count; i++)
                    {
                        _items.Remove(_prevItems[i]);
                        ObserveRemove().Nofity(new CollectionObserve<T>(index: i, newValue: _prevItems[i], oldValue: _prevItems[i]));
                    }
                    ObserveCount().Nofity(_items.Count);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            _prevItems = new List<T>(_items);
        }
    }
}