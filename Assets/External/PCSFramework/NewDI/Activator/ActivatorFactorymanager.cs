using System;

namespace PCS.DI.Activator
{
    internal static class ActivatorFactoryManager
    {
        internal static readonly IActivatorFactory Factory;

        static ActivatorFactoryManager()
        {
            Factory = GetFactory();
        }

        private static IActivatorFactory GetFactory()
        {

#if ENABLE_MONO
            return new MonoActivatorFactory();
#elif ENABLE_IL2CPP
            return new IL2CPPActivatorFactory();
#else
            throw new Exception("UndefinedRuntimeScriptingBackend");
#endif
        }
    }
}