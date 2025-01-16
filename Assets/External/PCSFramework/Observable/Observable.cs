using System;
using System.Threading;
namespace PCS.Observable
{
    public abstract class Observable<T>
    {
        public IDisposable Subscribe(Observer<T> observer)
        {
            try
            {
                var subscription = SubscribeCore(observer);
                //옵저버의 disposal에 옵저버 노드 할당. 옵저버가 dispose 될시
                observer.disposal = subscription;
                return observer;
            }
            catch
            {
                observer.Dispose();
                throw;
            }
        }

        public abstract void Nofity(T value);

        protected abstract IDisposable SubscribeCore(Observer<T> observer);

        public IDisposable Subscribe(Action<T> onNext)
        {
            return Subscribe(new AnonymousObserver<T>(onNext));
        }
    }

    public abstract class Observer<T> : IDisposable
    {
        internal IDisposable disposal;
        int _disposed;

        public bool IsDisposed => _disposed != 0;

        public void OnNext(T value)
        {
            if (IsDisposed) return;
            OnNextCore(value);
        }

        protected abstract void OnNextCore(T value);

        public void Dispose()
        {
            // 멀티스레딩에서 안전을 위해 사용. 0이 아니면 이미 Dispose 된 객체
            if ((Interlocked.Exchange(ref _disposed, 1) != 0)) return;

            DisposeCore();
            disposal?.Dispose();
        }

        protected virtual void DisposeCore() { }
    }

    public sealed class AnonymousObserver<T> : Observer<T>
    {
        private Action<T> _onNext;
        public AnonymousObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

        protected override void OnNextCore(T value)
        {
            _onNext(value);
        }
    }
    [Serializable]
    public readonly struct CollectionObserve<T>
    {
        public readonly int Index;
        public readonly T NewValue;
        public readonly T OldValue;
        public CollectionObserve(int index = -1, T newValue = default, T oldValue = default)
        {
            Index = index;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}