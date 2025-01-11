using System;

namespace PCS.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute
    {
        public string Name { get; }
        public InjectAttribute(string name = null) => Name = name;
    }
}
