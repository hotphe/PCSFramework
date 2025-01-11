using PCS.DI.Activator;
using System;

namespace PCS.DI.Cache
{
    internal sealed class TypeConstructionInfo
    {
        public readonly ObjectActivator ObjectActivator;
        public readonly Type[] ConstructorParameters;

        public TypeConstructionInfo(ObjectActivator objectActivator, Type[] constructorParameters)
        {
            ObjectActivator = objectActivator;
            ConstructorParameters = constructorParameters;
        }
    }
}