using System.Collections.Generic;
using System;
using PCS.DI.Resolver;
using PCS.DI.Bind;
using PCS.DI.Generic;
using PCS.DI.Extension;
using System.Linq;


namespace PCS.DI.Core
{
    public sealed class ContainerBuilder
    {
        public string Name { get; private set; }
        public Container Parent { get; private set; }
        
        public readonly List<Binding> Bindings = new List<Binding>();

        //Bind with type
        public Binding AddSingleton(Type concrete) => AddSingleton(concrete, concrete);
        public Binding AddSingleton<T>(params Type[] contracts) => AddSingleton(typeof(T),contracts);
        public Binding AddSingleton(Type concrete, params Type[] contracts)
        {
            var binding = Binding.Validated(new SingletonTypeResolver(concrete), concrete, contracts);
            Bindings.Add(binding);
            return binding;
        }

        public Binding AddScoped(Type concrete) => AddScoped(concrete, concrete);
        public Binding AddScoped<T>(params Type[] contracts) => AddScoped(typeof(T), contracts);
        public Binding AddScoped(Type concrete, params Type[] contracts)
        {
            var binding = Binding.Validated(new ScopedTypeResolver(concrete), concrete, contracts);
            Bindings.Add(binding);
            return binding;
        }

        public Binding AddTransient(Type concrete) => AddTransient(concrete, concrete);
        public Binding AddTransient<T>(params Type[] contracts) => AddTransient(typeof(T), contracts);
        public Binding AddTransient(Type concrete, params Type[] contracts)
        {
            var binding = Binding.Validated(new TransientTypeResolver(concrete), concrete, contracts);
            Bindings.Add(binding);
            return binding;
        }

        //Bind with instance(only singleton)

        public Binding AddSingleton(object instance) => AddSingleton(instance, instance.GetType());
        public Binding AddSingleton(object instance, params Type[] contracts)
        {
            var binding = Binding.Validated(new SingletonValueResolver(instance), instance.GetType(), contracts);
            Bindings.Add(binding);
            return binding;
        }

        public ContainerBuilder SetParent(Container parent)
        {
            Parent = parent;
            return this;
        }

        public ContainerBuilder SetName(string name)
        {
            Name = name;
            return this;
        }

        public Container Build()
        {
            var disposables = new DisposableCollection();
            var resolversByContract = new Dictionary<Type, List<IObjectResolver>>();
            var resolversByName = new Dictionary<Type, Dictionary<string, List<IObjectResolver>>>();

            // Inherited resolvers
            if (Parent != null)
            {
                foreach (var kvp in Parent.ResolversByContract)
                {
                    resolversByContract[kvp.Key] = kvp.Value.ToList();
                }
                
                foreach(var kvp in Parent.ResolversByName)
                {
                    var innerDictionary = new Dictionary<string, List<IObjectResolver>>();
                    foreach(var innerKvp in kvp.Value)
                    {
                        innerDictionary[innerKvp.Key] = innerKvp.Value.ToList();
                    }
                    resolversByName[kvp.Key] = innerDictionary;
                }
            }

            // Owned resolvers
            foreach (var binding in Bindings)
            {
                disposables.Add(binding.Resolver);
                if (binding.Name == null)
                {
                    foreach (var contract in binding.Contracts)
                        resolversByContract.GetOrAdd(contract, _ => new List<IObjectResolver>()).Add(binding.Resolver);
                }else
                {
                    foreach (var contract in binding.Contracts)
                    {
                        if(resolversByName.TryGetValue(contract,out var dict))
                        {
                            dict.GetOrAdd(binding.Name, _ => new List<IObjectResolver>()).Add(binding.Resolver);
                        }else
                        {
                            resolversByName.Add(contract, new Dictionary<string, List<IObjectResolver>>());
                            resolversByName[contract].GetOrAdd(binding.Name, _ => new List<IObjectResolver>()).Add(binding.Resolver);
                        }
                    }
                }
            }

            var container = new Container(Name, Parent, resolversByContract,resolversByName, disposables);
            
            return container;
        }

        public void Dispose()
        {
            
        }
    }
}