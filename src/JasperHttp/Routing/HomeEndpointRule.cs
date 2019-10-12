using System;
using System.Linq;
using System.Reflection;
using Baseline;

namespace JasperHttp.Routing
{
    internal class HomeEndpointRule : IPatternRule
    {
        public RoutePattern DetermineRoute(Type handlerType, MethodInfo method)
        {
            if (handlerType.Name == "HomeEndpoint" || handlerType.Name == "ServiceEndpoint")
            {
                var httpMethod = determineHttpMethodName(method);
                if (httpMethod.IsEmpty()) return null;

                return new RoutePattern(httpMethod, "/");
            }

            return null;
        }

        private string determineHttpMethodName(MethodInfo method)
        {
            if (method.Name.EqualsIgnoreCase("Index")) return "GET";

            if (HttpVerbs.All.Contains(method.Name, StringComparer.OrdinalIgnoreCase)) return method.Name.ToUpper();

            return null;
        }
    }
}
