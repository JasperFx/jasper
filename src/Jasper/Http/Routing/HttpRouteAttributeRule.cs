using System;
using System.Reflection;
using Baseline.Reflection;

namespace Jasper.Http.Routing
{
    internal class HttpRouteAttributeRule : IPatternRule
    {
        public RoutePattern DetermineRoute(Type handlerType, MethodInfo method)
        {
            var att = method.GetAttribute<HttpRouteAttribute>();
            if (att == null) return null;
            
            return new RoutePattern(att.Method, att.RoutePattern);
        }
    }
}