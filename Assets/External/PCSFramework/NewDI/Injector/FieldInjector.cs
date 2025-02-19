using System;
using PCS.DI.Cache;
using PCS.DI.Core;
using UnityEngine;

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
            {
                Debug.LogWarning($"Failed to inject {field.FieldInfo.Name} in {instance}.");
                return;
            }

            field.FieldInfo.SetValue(instance, resolvedInstance);
        }
    }
}