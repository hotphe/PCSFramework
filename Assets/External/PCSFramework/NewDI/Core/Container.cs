using PCS.DI.Generic;
using PCS.DI.Injector;
using PCS.DI.Resolver;
using PCS.DI.Extension;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.DI.Core
{
    public sealed class Container : IDisposable
    {
        public readonly string Name;
        internal readonly Container Parent;
        internal readonly List<Container> Children = new List<Container>();
        internal readonly Dictionary<Type, List<IObjectResolver>> ResolversByContract;
        internal readonly Dictionary<Type, Dictionary<string, List<IObjectResolver>>> ResolversByName;
        internal DisposableCollection Disposables;
        internal Container(string name, Container parent, Dictionary<Type,List<IObjectResolver>> resolversByContract, Dictionary<Type, Dictionary<string, List<IObjectResolver>>> resolversByName, DisposableCollection disposables)
        {
            Name = name;
            Parent = parent;
            Parent?.Children.Add(this);
            ResolversByContract = resolversByContract;
            ResolversByName = resolversByName;
            Disposables = disposables;
            OverrideSelfInjection();
        }

        public Container Scope(Action<ContainerBuilder> extend = null)
        {
            var builder = new ContainerBuilder().SetParent(this);
            extend?.Invoke(builder);
            return builder.Build();
        }

        public object Construct(Type concrete)
        {
            var instance = ConstructorInjector.Construct(concrete, this);
            AttributeInjector.Inject(instance,this);
            return instance;
        }

        public object Resolve(Type type)
        {
            if (type.IsEnumerable(out var elementType))
            {
                return All(elementType).CastDynamic(elementType);
            }
            var resolvers = GetResolvers(type);
            var lastResolver = resolvers?.Last();
            var resolved = lastResolver?.Resolve(this);
            return resolved;
            
        }
        private IEnumerable<IObjectResolver> GetResolvers(Type contract)
        {
            if (ResolversByContract.TryGetValue(contract, out var resolvers))
            {
                return resolvers;
            }
            return null;
        }

        public object Resolve(Type type, string name)
        {
            if (type.IsEnumerable(out var elementType))
            {
                return All(name, elementType).CastDynamic(elementType);
            }
            var resolvers = GetResolvers(type,name);
            var lastResolver = resolvers.Last();
            var resolved = lastResolver.Resolve(this);
            return resolved;
        }

        private IEnumerable<IObjectResolver> GetResolvers(Type contract, string name)
        {
            if(ResolversByName.TryGetValue(contract, out var dict))
            {
                if (dict.TryGetValue(name, out var resolvers))
                    return resolvers;
            }
            return null;
        }

        public IEnumerable<object> All(Type contract)
        {
            return ResolversByContract.TryGetValue(contract, out var resolvers)
                ? resolvers.Select(resolver => resolver.Resolve(this)).ToArray()
                : Enumerable.Empty<object>();
        }

        public IEnumerable<TContract> All<TContract>()
        {
            return ResolversByContract.TryGetValue(typeof(TContract), out var resolvers)
                ? resolvers.Select(resolver => (TContract)resolver.Resolve(this)).ToArray()
                : Enumerable.Empty<TContract>();
        }

        public IEnumerable<object> All(string name, Type contract)
        {
            if (ResolversByName.TryGetValue(contract, out var dict))
            {
                return dict.TryGetValue(name, out var resolvers)
                    ? resolvers.Select(resolver => resolver.Resolve(this)).ToArray()
                    : Enumerable.Empty<object>();
            }
            return Enumerable.Empty<object>();
        }

        public IEnumerable<TContract> All<TContract>(string name)
        {
            if (ResolversByName.TryGetValue(typeof(TContract), out var dict))
            {
                return dict.TryGetValue(name, out var resolvers)
                    ? resolvers.Select(resolver => (TContract)resolver.Resolve(this)).ToArray()
                    : Enumerable.Empty<TContract>();
            }
            return Enumerable.Empty<TContract>();
        }

        public void Dispose()
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].Dispose();
            }

            Parent?.Children.Remove(this);
            ResolversByContract.Clear();
            ResolversByName.Clear();
            Disposables.Dispose();
        }

        private void OverrideSelfInjection()
        {
            ResolversByContract[typeof(Container)] = new List<IObjectResolver> { new SingletonValueResolver(this) };
        }

    }
}