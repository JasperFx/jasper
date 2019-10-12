using System;
using System.Linq;
using JasperHttp.Model;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace JasperHttp
{
    internal class RouteScopingPolicy : IFamilyPolicy
    {
        private readonly RouteGraph _routes;

        public RouteScopingPolicy(RouteGraph routes)
        {
            _routes = routes;
        }

        private bool matches(Type type)
        {
            return _routes.ToArray().Any(x => x.Action.HandlerType == type);
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsConcrete() && _routes.IsSealed() && matches(type))
            {
                var instance = new ConstructorInstance(type, type, ServiceLifetime.Scoped);
                return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
            }

            return null;
        }
    }
}
