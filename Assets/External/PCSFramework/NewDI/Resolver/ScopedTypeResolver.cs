using System.Runtime.CompilerServices;
using System;
using PCS.DI.Core;

namespace PCS.DI.Resolver
{
    public sealed class ScopedTypeResolver : IObjectResolver
    {
        private readonly Type _concreteType;
        private readonly ConditionalWeakTable<Container, object> _instances = new(); //If the container is collected by the GC, the object will also be collected.
        public Lifetime Lifetime => Lifetime.Scoped;

        public ScopedTypeResolver(Type concreteType)
        {
            _concreteType = concreteType;
        }

        public object Resolve(Container container)
        {
            if (!_instances.TryGetValue(container, out var instance))
            {
                instance = container.Construct(_concreteType);
                _instances.Add(container, instance);
                container.Disposables.TryAdd(instance);
                return instance;
               
            }
            return instance;
        }

        public void Dispose()
        {
        }
    }
}