using System;
using System.Reflection;

namespace JasperHttp.Routing
{
    public interface IPatternRule
    {
        /// <summary>
        /// Return null if the rule does not match
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        RoutePattern DetermineRoute(Type handlerType, MethodInfo method);
    }
}
