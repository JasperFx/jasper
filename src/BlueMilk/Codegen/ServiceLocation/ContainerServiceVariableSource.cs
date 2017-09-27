using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Codegen.ServiceLocation
{
    public class ContainerServiceVariableSource : IVariableSource
    {
        private readonly IServiceCollection _services;

        public ContainerServiceVariableSource(IServiceCollection services)
        {
            _services = services;
        }

        public bool Matches(Type type)
        {
            return type == typeof(IServiceScopeFactory) || type == typeof(IServiceProvider) || _services.Any(x => x.ServiceType == type);
        }

        public Variable Create(Type type)
        {
            if (type == typeof(IServiceScopeFactory))
            {
                return new InjectedField(typeof(IServiceScopeFactory));
            }

            if (type == typeof(IServiceProvider))
            {
                return new ServiceScopeFactoryCreation().Provider;
            }

            return new ServiceCreationFrame(type).Service;
        }
    }
}
