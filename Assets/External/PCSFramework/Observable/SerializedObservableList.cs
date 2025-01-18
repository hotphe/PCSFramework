using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace PCS.Observable
{
    [Serializable]
    public class SerializedObservableList<T> : ObservableList<T>
    {
        // 만약 T가 struct 일 때, Inspector에서 Remove 시 몇번째 요소가 사라졌는지 확인 불가하므로 ID 할당
        [Serializable]
        public class ValueMap
        {
            public int Id;
            public T Value;

            public ValueMap(int id, T value)
            {
                Id = id;
                Value = value;
            }
        }
        [HideInInspector] [SerializeField] private int _counter = 1; // id할당에 사용되는 카운터
        [HideInInspector] [SerializeField] private int _prevSize;

        [SerializeField] private List<ValueMap> _newItems;
        private List<ValueMap> _prevItems = new List<ValueMap>();

        private void ForceChange()
        {
            if (_items.Count != _newItems.Count)
            {
                if (_items.Count < _newItems.Count) // 인스펙터에서 추가했을경우
                {
                    for (int i = 0; i < _items.Count; i++)
                        _newItems[i].Value = _items[i];

                    for (int i = _items.Count; i < _newItems.Count; i++)
                        Add(_newItems[i].Value);
                }
                else if (_items.Count > _newItems.Count) // 인스펙터에서 삭제했을경우
                {
                    var indexes = _prevItems
                        .Select((item, index) => new { Item = item, Index = index })
                        .Where(x => !_newItems.Any(t2Item => t2Item.Id == x.Item.Id))
                        .Select(x => x.Index)
                        .ToList();

                    // 역순으로 정렬
                    indexes = indexes.OrderByDescending(x => x).ToList();

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        RemoveAt(indexes[i]);
                    }

                    for (int i = 0; i < _newItems.Count; i++)
                        _newItems[i].Value = _items[i];
                }
            }
            else
            {
                for (int i = 0; i < _newItems.Count; i++)
                {
                    if (_prevItems[i].Id.Equals(_newItems[i].Id))
                        continue;
                    this[i] = _newItems[i].Value;
                }
            }

            _prevItems = new List<ValueMap>();
            ValueMap vm;
            foreach (var item in _newItems)
            {
                vm = new ValueMap(item.Id, item.Value);
                _prevItems.Add(vm);
            }
            _prevSize = _prevItems.Count;
        }
    }
}