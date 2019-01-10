using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.MvcExtender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

[assembly:JasperModule(typeof(MvcExtenderExtension))]

namespace Jasper.MvcExtender
{
    public class MvcExtenderExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.HttpRoutes.IncludeTypes(x => x.CanBeCastTo<ControllerBase>());
            registry.HttpRoutes.IncludeMethods(x => x.HasAttribute<HttpMethodAttribute>());

            RouteBuilder.PatternRules.Insert(0, new HttpAttributePatternRule());
        }
    }

    public class HttpAttributePatternRule : IPatternRule
    {
        public RoutePattern DetermineRoute(Type handlerType, MethodInfo method)
        {
            if (method.HasAttribute<HttpMethodAttribute>())
            {
                var att = method.GetAttribute<HttpMethodAttribute>();
                var httpMethod = att.HttpMethods.Single();

                var pattern = att.Template;

                return new RoutePattern(httpMethod, pattern)
                {
                    Order = att.Order
                };
            }

            return null;
        }
    }
}
