using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Codegen;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Model
{
    public class RouteGraph : HandlerSet<RouteChain, HttpContext, RouteHandler>
    {
        public static readonly string Context = "context";


        private readonly IList<RouteChain> _chains = new List<RouteChain>();

        protected override RouteChain[] chains => _chains.ToArray();


        public void AddRoute(Type handlerType, MethodInfo method)
        {
            var route = new RouteChain(new MethodCall(handlerType, method));
            _chains.Add(route);
        }
    }
}
