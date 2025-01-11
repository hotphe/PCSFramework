using System.Reflection;

namespace PCS.DI.Cache
{
    internal sealed class InjectablePropertyInfo
    {
        public readonly PropertyInfo PropertyInfo;
        public readonly string Name;
        public InjectablePropertyInfo(PropertyInfo propertyInfo, string name = null)
        {
            PropertyInfo = propertyInfo;
            Name = name;
        }
    }
}
