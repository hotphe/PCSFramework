using System;
using PCS.DI.Generic;
using PCS.DI.Core;

namespace PCS.DI.Resolver
{
    public sealed class SingletonTypeResolver : IObjectResolver
    {
        private object _instance;
        private readonly Type _concreteType;
        private readonly DisposableCollection _disposables = new DisposableCollection();
        public Lifetime Lifetime => Lifetime.Singleton;

        public SingletonTypeResolver(Type concreteType)
        {
            _concreteType = concreteType;
        }
        public object Resolve(Container container)
        {
            if(_instance == null)
            {
                _instance = container.Construct(_concreteType);
                _disposables.TryAdd(_instance);
            }
            return _instance;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
