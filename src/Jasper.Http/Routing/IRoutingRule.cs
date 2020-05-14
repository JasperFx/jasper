using System;
using System.Reflection;

namespace Jasper.Http.Routing
{
    public interface IRoutingRule
    {
        /// <summary>
        ///     Return null if the rule does not match
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        JasperRoute DetermineRoute(Type handlerType, MethodInfo method);
    }
}
