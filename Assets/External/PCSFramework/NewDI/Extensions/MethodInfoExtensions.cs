using System;
using System.Reflection;

namespace PCS.DI.Extension
{
    internal static class MethodInfoExtensions
    {
        public static T CreateDelegate<T>(this MethodInfo methodInfo) where T : Delegate
        {
            return (T)methodInfo.CreateDelegate(typeof(T));
        }
    }
}