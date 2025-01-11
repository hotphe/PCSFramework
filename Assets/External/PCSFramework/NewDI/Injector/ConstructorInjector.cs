using System;
using System.Buffers;
using PCS.DI.Core;
using PCS.DI.Cache;

namespace PCS.DI.Injector
{
    public static class ConstructorInjector
    {
        public static object Construct(Type concrete, Container container)
        {
            var info = TypeConstructionInfoCache.Get(concrete);
            var arguments = ArrayPool<object>.Shared.Rent(info.ConstructorParameters.Length);

            for (var i = 0; i < info.ConstructorParameters.Length; i++)
            {
                arguments[i] = container.Resolve(info.ConstructorParameters[i]);
            }

            try
            {
                var instance = info.ObjectActivator.Invoke(arguments);
                return instance;
            }
            catch (Exception e)
            {
                throw new Exception($"Faild to instantiate object type {concrete.Name}. \n{e.Message}");
            }
            finally
            {
               ArrayPool<object>.Shared.Return(arguments);
            }
        }

        public static object Construct(Type concrete, object[] arguments)
        {
            var info = TypeConstructionInfoCache.Get(concrete);

            try
            {
                return info.ObjectActivator.Invoke(arguments);
            }
            catch (Exception e)
            {
                throw new Exception($"Faild to instantiate object type {concrete.Name}. \n{e.Message}");
            }
        }
    }
}