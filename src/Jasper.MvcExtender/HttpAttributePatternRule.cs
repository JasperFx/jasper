using System;
using System.Linq;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Jasper.MvcExtender
{
    public class HttpAttributePatternRule : IPatternRule
    {
        public RoutePattern DetermineRoute(Type handlerType, MethodInfo method)
        {
            if (method.HasAttribute<HttpMethodAttribute>())
            {
                var att = method.GetAttribute<HttpMethodAttribute>();
                var httpMethod = att.HttpMethods.Single();

                var pattern = RoutePrefixFor(handlerType) + att.Template ?? "";



                return new RoutePattern(httpMethod, pattern)
                {
                    Order = att.Order
                };
            }

            return null;
        }

        public static string RoutePrefixFor(Type handlerType)
        {
            return handlerType.HasAttribute<RouteAttribute>()
                ? handlerType.GetAttribute<RouteAttribute>().Template
                    .Replace("[controller]", handlerType.Name.Replace("Controller", ""))
                : string.Empty;
        }
    }
}
