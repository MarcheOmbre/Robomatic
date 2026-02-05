using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Project.Scripts.Utils
{
    public static class ReflectionExtensions
    {
        // https://stackoverflow.com/questions/940675/getting-a-delegate-from-methodinfo
        public static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
        {
            var parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
            var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
            var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

            return methodInfo.IsStatic ? methodInfo.CreateDelegate(delegateType) : methodInfo.CreateDelegate(delegateType, target);
        }
    }
}