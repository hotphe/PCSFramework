namespace PCS.DI.Cache
{
    internal sealed class TypeAttributeInfo
    {
        public readonly InjectableFieldInfo[] InjectableFields;
        public readonly InjectablePropertyInfo[] InjectableProperties;
        public readonly InjectableMethodInfo[] InjectableMethods;

        public TypeAttributeInfo(InjectableFieldInfo[] injectableFields, InjectablePropertyInfo[] injectableProperties, InjectableMethodInfo[] injectableMethods)
        {
            InjectableFields = injectableFields;
            InjectableProperties = injectableProperties;
            InjectableMethods = injectableMethods;
        }

    }
}