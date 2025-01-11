using PCS.DI.Generic;
using PCS.DI.Core;

namespace PCS.DI.Resolver
{
    public sealed class SingletonValueResolver : IObjectResolver
    {
        private readonly object _instance;
        private readonly DisposableCollection _disposables = new DisposableCollection();
        public Lifetime Lifetime => Lifetime.Singleton;

        public SingletonValueResolver(object instance)
        {
            _instance = instance;
            _disposables.TryAdd(instance);
        }
        public object Resolve(Container container)
        {
            return _instance;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
