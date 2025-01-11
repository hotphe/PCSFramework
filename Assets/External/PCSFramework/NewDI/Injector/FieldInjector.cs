using System;
using PCS.DI.Cache;
using PCS.DI.Core;

namespace PCS.DI.Injector
{
    internal static class FieldInjector
    {
        internal static void Inject(InjectableFieldInfo field, object instance, Container container)
        {
            var resolvedInstance = field.Name != null 
                ? container.Resolve(field.FieldInfo.FieldType, field.Name)
                : container.Resolve(field.FieldInfo.FieldType);
            if (resolvedInstance == null)
                throw new Exception($"Failed to inject {field.Name}.");

            field.FieldInfo.SetValue(instance, resolvedInstance);
        }
    }
}