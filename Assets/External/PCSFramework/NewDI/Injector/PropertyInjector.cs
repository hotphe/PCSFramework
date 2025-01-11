using System;
using PCS.DI.Cache;
using PCS.DI.Core;

namespace PCS.DI.Injector
{
    internal static class PropertyInjector
    {
        internal static void Inject(InjectablePropertyInfo property, object instance, Container continaer)
        {
            var resolvedInstance = property.Name != null
                ? continaer.Resolve(property.PropertyInfo.PropertyType, property.Name)
                : continaer.Resolve(property.PropertyInfo.PropertyType);
            if (resolvedInstance == null)
                throw new Exception($"Failed to inject {property.Name}.");
            property.PropertyInfo.SetValue(instance, resolvedInstance);
        }
    }
}