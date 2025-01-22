using System.Collections.Generic;
using PCS.Common;
using System;
namespace PCS.Observable
{
    public class ObservableDictionary<TKey, TValue> : SerializedDictionary<TKey, TValue>, IDisposable
    {
        private bool _disposed;
        public bool IsDisposed => _disposed;

        private ObservableProperty<int> _onCountChangeObservable = new ObservableProperty<int>();
        private ObservableProperty<KeyValuePair<TKey, TValue>> _onAddObservable = new ObservableProperty<KeyValuePair<TKey, TValue>>();
        private ObservableProperty<KeyValuePair<TKey, TValue>> _onInsertObservable = new ObservableProperty<KeyValuePair<TKey, TValue>>();
        private ObservableProperty<KeyValuePair<TKey, TValue>> _onRemoveObservable = new ObservableProperty<KeyValuePair<TKey, TValue>>();

        public Observable<int> ObserveCount() => _onCountChangeObservable;
        public Observable<KeyValuePair<TKey, TValue>> ObserveAdd() => _onAddObservable;
        public Observable<KeyValuePair<TKey, TValue>> ObserveInsert() => _onInsertObservable;
        public Observable<KeyValuePair<TKey, TValue>> ObserveRemove() => _onRemoveObservable;

        public ObservableDictionary()
        {
            SerializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>();
        }

        public new TValue this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                ThrowIfDisposed();

                if (base.TryGetValue(key, out var oldValue))
                {
                    base[key] = value;
                    _onInsertObservable.Notify(new SerializedKeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    base[key] = value;
                    _onAddObservable.Notify(new SerializedKeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        public new void Add(TKey key, TValue value)
        {
            ThrowIfDisposed();
            base.Add(key, value);
            _onAddObservable.Notify(new KeyValuePair<TKey, TValue>(key, value));
        }


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