using System;

namespace PCS.DI
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute
    {
        public LifecycleScope Scope;
        public ProvideAttribute(LifecycleScope scope) 
        { 
            Scope = scope;
        }
    }
}
