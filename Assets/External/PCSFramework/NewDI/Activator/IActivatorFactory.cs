using System.Reflection;
using System;

namespace PCS.DI.Activator
{
    internal interface IActivatorFactory
    {
        ObjectActivator GenerateActivator(Type type, ConstructorInfo constructor, Type[] parameters);
        ObjectActivator GenerateDefaultActivator(Type type);
    }
}
