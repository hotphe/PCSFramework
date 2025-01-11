using System;
using PCS.DI.Core;

namespace PCS.DI.Resolver
{
    public sealed class TransientTypeResolver : IObjectResolver
    {
        private readonly Type _concreteType;
        public Lifetime Lifetime => Lifetime.Transient;

        public TransientTypeResolver(Type concreteType)
        {
            _concreteType = concreteType;
        }

        public object Resolve(Container container)
        {
            var instance = container.Construct(_concreteType);
            container.Disposables.TryAdd(instance);
            return instance;
        }

        public void Dispose()
        {
        }
    }
}