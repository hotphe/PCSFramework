using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PCS.DI.Cache
{
    internal static class TypeInfoCache
    {
        private const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private static readonly List<InjectableFieldInfo> _fields = new();
        private static readonly List<InjectablePropertyInfo> _properties = new();
        private static readonly List<InjectableMethodInfo> _methods = new();

        private static readonly Dictionary<Type, TypeAttributeInfo> _dictionary = new();

        internal static TypeAttributeInfo Get(Type type)
        {
            if (!_dictionary.TryGetValue(type, out var info))
            {
                _fields.Clear();
                _properties.Clear();
                _methods.Clear();
                Generate(type);
                info = new TypeAttributeInfo(_fields.ToArray(), _properties.ToArray(), _methods.ToArray());
                _dictionary.Add(type, info);
            }

            return info;
        }

        private static void Generate(Type type)
        {
            var fields = type
                .GetFields(_bindingFlags)
                .Where(f => f.IsDefined(typeof(InjectAttribute)))
                .Select(f => new InjectableFieldInfo(f, f.GetCustomAttribute<InjectAttribute>().Name));

            var properties = type
                .GetProperties(_bindingFlags)
                .Where(p => p.CanWrite && p.IsDefined(typeof(InjectAttribute)))
                .Select(p => new InjectablePropertyInfo(p, p.GetCustomAttribute<InjectAttribute>().Name));

            var methods = type
                .GetMethods(_bindingFlags)
                .Where(m => m.IsDefined(typeof(InjectAttribute)))
                .Select(m => new InjectableMethodInfo(m));

            _fields.AddRange(fields);
            _properties.AddRange(properties);
            _methods.AddRange(methods);

            if (type.BaseType != null)
            {
                Generate(type.BaseType);
            }
        }
    }
}