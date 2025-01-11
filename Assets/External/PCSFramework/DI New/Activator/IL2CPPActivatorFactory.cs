using System;
using System.Reflection;

namespace PCS.DI.Activator
{
    internal sealed class IL2CPPActivatorFactory : IActivatorFactory
    {
        public ObjectActivator GenerateActivator(Type type, ConstructorInfo constructor, Type[] parameters)
        {
            return args =>
            {
                var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                constructor.Invoke(instance, args);
                return instance;
            };
        }

        public ObjectActivator GenerateDefaultActivator(Type type)
        {
            return args => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        }
    }
}