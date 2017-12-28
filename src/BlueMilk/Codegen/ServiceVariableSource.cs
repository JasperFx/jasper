using System;
using BlueMilk.Codegen.ServiceLocation;
using BlueMilk.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Codegen
{
    public class ServiceVariableSource : IVariableSource
    {
        private readonly IMethodVariables _method;
        private readonly ServiceGraph _services;

        public ServiceVariableSource(IMethodVariables method, ServiceGraph services)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));
            _services = services;
        }

        public bool Matches(Type type)
        {
            // TODO -- Do we really want to do this this way?
            return true;
        }

        public Variable Create(Type type)
        {
            var @default = _services.FindDefault(type);
            BuildStepPlanner planner = null;
            if (@default?.ImplementationType != null && @default.Lifetime != ServiceLifetime.Singleton)
            {
                planner = new BuildStepPlanner(type, @default.ImplementationType, _services, _method);
            }


            return new ServiceCreationFrame(type, planner).Service;
        }
    }
}
