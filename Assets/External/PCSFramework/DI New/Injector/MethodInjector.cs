using System;
using System.Linq;
using PCS.DI.Cache;
using PCS.DI.Core;

namespace PCS.DI.Injector
{
    internal static class MethodInjector
    {
        internal static void Inject(InjectableMethodInfo method, object instance, Container container)
        {
            var resolvedInstances = method.Parameters.Select(container.Resolve).ToArray();

            if (resolvedInstances.Any(memeber => memeber == null))
                throw new Exception($"Failed to inject {method.MethodInfo.Name}.");
            method.MethodInfo.Invoke(instance, resolvedInstances);
        }
    }
}