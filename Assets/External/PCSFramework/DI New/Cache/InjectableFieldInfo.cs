using System.Reflection;

namespace PCS.DI.Cache
{
    internal sealed class InjectableFieldInfo
    {
        public readonly FieldInfo FieldInfo;
        public readonly string Name;
        public InjectableFieldInfo(FieldInfo fieldInfo, string name = null)
        {
            FieldInfo = fieldInfo;
            Name = name;
        }
    }
}
