using System;
using PCS.DI.Resolver;

namespace PCS.DI.Bind
{
    public sealed class Binding
    {
        public IObjectResolver Resolver;
        public string Name;
        public readonly Type[] Contracts;
        

        public Binding(IObjectResolver resolver, Type[] contracts)
        {
            Resolver = resolver;
            Contracts = contracts;
        }

        public static Binding Validated(IObjectResolver resolver, Type concrete, params Type[] contracts)
        {
            foreach (var contract in contracts)
            {
                if (!contract.IsAssignableFrom(concrete))
                {
                    throw new Exception($"Concrete class({concrete.Name}) does not implement or inherit  from the contract interface or abstract class({contract.Name}). ");
                }
            }
            return new Binding(resolver, contracts);
        }

        public Binding WithName(string name)
        {
            Name = name;
            return this;
        }
    }
}