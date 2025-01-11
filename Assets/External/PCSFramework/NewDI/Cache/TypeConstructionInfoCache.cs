using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PCS.DI.Activator;

namespace PCS.DI.Cache
{
    internal static class TypeConstructionInfoCache
    {
        private static readonly Dictionary<Type, TypeConstructionInfo> _dictionary = new();

        internal static TypeConstructionInfo Get(Type type)
        {
            if (!_dictionary.TryGetValue(type, out var info))
            {
                info = Generate(type);
                _dictionary.Add(type, info);
            }

            return info;
        }

        private static TypeConstructionInfo Generate(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length >0)
            {
                var constructor = constructors.OrderByDescending(ctor => ctor.GetParameters().Length).First();

                var parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                return new TypeConstructionInfo(ActivatorFactoryManager.Factory.GenerateActivator(type, constructor, parameters), parameters);
            }

            return new TypeConstructionInfo(ActivatorFactoryManager.Factory.GenerateDefaultActivator(type), Type.EmptyTypes);
        }
    }
}