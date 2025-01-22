using System;
using System.Collections.Generic;
using System.Threading;
namespace PCS.Observable
{
    public class ObservableProperty<T> : Observable<T>, IDisposable
    {
        private ObserverNode _root;
        private bool _disposed;
        public bool IsDisposed => _disposed;

        public ObservableProperty() : this(default) { }

        public ObservableProperty(T value) : this(value, EqualityComparer<T>.Default)
        {
        }

        public ObservableProperty(T value, IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
            _value = value;
        }
        
        public override void Notify(T value)
        {
            OnValueChanged(value);
        }

        protected override void OnValueChanged(T value)
        {
            ThrowIfDisposed();

            var node = _root;
            var last = node?.Previous;
            while (node != null)
            {
                node.Observer.OnNext(value);
                if (node == last) return;
                node = node.Next;
            }
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            lock (this) // lock(this) 보다 private object를 생성해서 거는게 좋지만 R3 기준 이렇게 구현되어있음.
            {
                ThrowIfDisposed();
                var subscription = new ObserverNode(this, observer);

                ObserverNode node = Volatile.Read(ref _root);

                return subscription;
            }
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            CleareNodes();
            _disposed = true;
        }

        public void CleareNodes()
        {
            lock (this)
            {
                ObserverNode node = Volatile.Read(ref _root);

                while (node != null)
                {
                    node.Observer.Dispose();
                    node = node.Next;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("");
        }

        public class ObserverNode : IDisposable
        {
            public readonly Observer<T> Observer;

            public ObservableProperty<T> Parent;

            public ObserverNode Previous { get; set; }
            public ObserverNode Next { get; set; }

            public ObserverNode(ObservableProperty<T> parent, Observer<T> observer)
            {
                Parent = parent;
                Observer = observer;

                if (parent._root == null)
                {
                    Volatile.Write(ref parent._root, this);
                }
                else
                {
                    var lastNode = parent._root.Previous ?? parent._root;
                    // 이전 노드(없으면 루트)의 다음을 나로, 나의 이전을 이전 노드(없으면 루트)로, 루트노드의 이전은 나(마지막으로 추가된 노드.)
                    // 양방향 연결 리스트와 비슷하나 root의 이전이 마지막 노드로 할당되어 있음.
                    lastNode.Next = this;
                    this.Previous = lastNode;
                    parent._root.Previous = this;
                }
            }

            public void Dispose()
            {
                var p = Interlocked.Exchange(ref Parent, null);
                if (p == null || p.IsDisposed) return;

                lock (p)
                {
                    if (p._root == null) return;

                    // 이해가 안된다면 양방향 연결 리스트를 공부하고 오는 것을 추천함.
                    if (this == p._root)
                    {
                        if (this.Previous == null || this.Next == null)
                            p._root = null;
                        else
                        {
                            var root = this.Next;// 다음 노드를 root로 변경할 준비
                            if (root.Next == null)
                                root.Previous = null;// 다음 노드의 다음이 없으면 (루트노드와 다음 노드만 존재), 이전인 나를 null처리
                            else
                                root.Previous = this.Previous; // 다음 노드의 다음이 있으면(루트노드 포함 노드가 3개이상), 내 이전(마지막 노드)를 다음 노드의 Previous로 변경

                            p._root = root;
                        }
                    }
                    else
                    {
                        //루트가 아니면, 무조건 이전 노드가 존재(루트일경우, 노드가 루트 단 하나일 경우 존재하지 않을 수 있음)  이전노드의 다음을 내 다음 노드로 변경
                        this.Previous!.Next = this.Next;
                        if (this.Next != null)
                        {
                            this.Next.Previous = this.Previous; // 내 다음 노드가 null이 아니면, 내 다음 노드의 이전노드를 내 이전 노드로 변경
                        }
                        else
                        {
                            p._root!.Previous = this.Previous; // 내 다음 노드가 null이면(내가 마지막 노드이면), root의 이전 노드(마지막 노드)를 내 이전 노드로 변경
                        }
                    }
                }

            }
        }

    }
}