using System;
using System.Linq;
using Jasper.Http.Model;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    /// <summary>
    /// This ensures that any concrete type that services HTTP
    /// requests with Jasper is registered in the Lamar container
    /// as "Scoped"
    /// </summary>
    internal class RouteScopingPolicy : IFamilyPolicy
    {
        private readonly RouteGraph _routes;

        public RouteScopingPolicy(RouteGraph routes)
        {
            _routes = routes;
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsConcrete() && matches(type))
            {
                var instance = new ConstructorInstance(type, type, ServiceLifetime.Scoped);
                return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
            }

            return null;
        }

        private bool matches(Type type)
        {
            return _routes.ToArray().Any(x => x.Action.HandlerType == type);
        }
    }
}
