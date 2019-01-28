using System;
using System.Linq;
using Baseline;
using Lamar;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jasper.Util.Lamar
{
    internal class OptionsPolicy : IRegistrationPolicy, IFamilyPolicy
    {
        public static bool Matches(ServiceDescriptor descriptor)
        {
            return  descriptor.ServiceType.Closes(typeof(IOptions<>))
                    && !descriptor.ServiceType.IsOpenGeneric()
                    && descriptor.ImplementationType.Closes(typeof(OptionsManager<>));
        }
        
        public void Apply(ServiceRegistry registry)
        {
            for (var index = registry.Count - 1; index >= 0; --index)
            {
                if (!Matches(registry[index])) continue;

                var openType = registry[index].ServiceType.Closes(typeof(IOptionsSnapshot<>))
                    ? typeof(OptionsSnapshotInstance<>)
                    : typeof(OptionsInstance<>);
                
                var type = registry[index].ServiceType.FindParameterTypeTo(typeof(IOptions<>));
                var instance = openType.CloseAndBuildAs<Instance>(type);
                registry[index] = instance.ToDescriptor();


            }
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.Closes(typeof(IOptionsSnapshot<>)))
            {
                var argType = type.GetGenericArguments().Single();
                var instance = typeof(OptionsSnapshotInstance<>).CloseAndBuildAs<Instance>(argType);
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, instance);
            }
            
            if (type.Closes(typeof(IOptions<>)))
            {
                var argType = type.GetGenericArguments().Single();
                var instance = typeof(OptionsInstance<>).CloseAndBuildAs<Instance>(argType);
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, instance);
            }

            return null;
        }
    }
}