using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace PCS.Observable
{
    [Serializable]
    public class SerializableObservableList<T> : IList<T>, IDisposable
    {
        [SerializeField] private List<T> _items;

        private bool _disposed;
        public bool IsDisposed => _disposed;

        private ObservableProperty<int> _onCountChangeObservable = new ObservableProperty<int>();
        private ObservableProperty<CollectionObserve<T>> _onAddObservable = new ObservableProperty<CollectionObserve<T>>();
        private ObservableProperty<CollectionObserve<T>> _onInsertObservable = new ObservableProperty<CollectionObserve<T>>();
        private ObservableProperty<CollectionObserve<T>> _onRemoveObservable = new ObservableProperty<CollectionObserve<T>>();
        private ObservableProperty<CollectionObserve<T>> _onValueChangeObservable = new ObservableProperty<CollectionObserve<T>>();

        public Observable<int> ObserveCount() => _onCountChangeObservable;
        public Observable<CollectionObserve<T>> ObserveAdd() => _onAddObservable;
        public Observable<CollectionObserve<T>> ObserveInsert() => _onInsertObservable;
        public Observable<CollectionObserve<T>> ObserveRemove() => _onRemoveObservable;
        public Observable<CollectionObserve<T>> ObserveValueChange() => _onValueChangeObservable;

        public SerializableObservableList()
        {
            _items = new List<T>();
        }

        public SerializableObservableList(int capacity)
        {
            _items = new List<T>(capacity);
        }

        public SerializableObservableList(IEnumerable<T> collection)
        {
            _items = collection.ToList();
        }

        public virtual T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                ThrowIfDisposed();
                var oldValue = _items[index];
                _items[index] = value;
                _onValueChangeObservable.Notify(new CollectionObserve<T>(index: index, newValue: value, oldValue: oldValue));
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public virtual void Add(T item)
        {
            ThrowIfDisposed();
            _items.Add(item);
            _onAddObservable.Notify(new CollectionObserve<T>(index: _items.Count, newValue: item));
            _onCountChangeObservable.Notify(Count);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection), "The collection cannot be null.");

            if (collection.Count() > 0)
            {
                foreach (T item in collection)
                {
                    _items.Add(item);
                    _onAddObservable.Notify(new CollectionObserve<T>(index: _items.Count, newValue: item));
                }
                _onCountChangeObservable.Notify(Count);
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            var itemsToRemove = new List<T>(_items);
            foreach (var item in itemsToRemove)
                Remove(item);
            _items.Clear();
            _onCountChangeObservable.Notify(Count);
        }

        public bool Contains(T item)
        {
            try
            {
                ThrowIfDisposed();
                return _items.Contains(item);
            }
            catch
            {
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowIfDisposed();
            _items.CopyTo(array, arrayIndex);
        }
        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                ThrowIfDisposed();
                return _items.GetEnumerator();
            }
            catch
            {
                return null;
            }
        }

        public int IndexOf(T item)
        {
            try
            {
                ThrowIfDisposed();
                return _items.IndexOf(item);
            }
            catch
            {
                return -1;
            }
        }

        public virtual void Insert(int index, T item)
        {
            ThrowIfDisposed();
            var oldValue = _items[index];
            _items.Insert(index, item);
            _onInsertObservable.Notify(new CollectionObserve<T>(index: index, newValue: item, oldValue: oldValue));
        }

        public virtual bool Remove(T item)
        {
            ThrowIfDisposed();
            var index = _items.IndexOf(item);
            bool removed = _items.Remove(item);
            if (removed)
            {
                _onRemoveObservable.Notify(new CollectionObserve<T>(index: index, newValue: item, oldValue: item));
                _onCountChangeObservable.Notify(Count);
            }
            return removed;
        }

        public virtual void RemoveAt(int index)
        {
            ThrowIfDisposed();
            if (index >= _items.Count)
            {
                Debug.LogWarning($"Index({index}) out of range. No item was removed from the collection.");
                return;
            }
            T item = _items[index];
            _items.RemoveAt(index);
            _onRemoveObservable.Notify(new CollectionObserve<T>(index: index, newValue: item, oldValue: item));
            _onCountChangeObservable.Notify(Count);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            ThrowIfDisposed();
            _onCountChangeObservable.Dispose();
            _onAddObservable.Dispose();
            _onInsertObservable.Dispose();
            _onRemoveObservable.Dispose();
        }
        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("");
        }
    }
}