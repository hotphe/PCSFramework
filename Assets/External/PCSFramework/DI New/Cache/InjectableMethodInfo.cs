using System.Reflection;
using System;
using System.Linq;

namespace PCS.DI.Cache
{
    internal sealed class InjectableMethodInfo
    {
        public readonly MethodInfo MethodInfo;
        public readonly Type[] Parameters;
        public InjectableMethodInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            Parameters = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        }
    }
}
