using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Bus;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypeClassification = BlueMilk.Scanning.TypeClassification;
using TypeRepository = BlueMilk.Scanning.TypeRepository;

namespace Jasper.Conneg
{
    public class Forwarders
    {
        private readonly LightweightCache<Type, List<Type>> _forwarders = new LightweightCache<Type, List<Type>>(t => new List<Type>());

        public void Add(Type type)
        {
            if (!type.HasAttribute<VersionAttribute>())
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"'Forwarding' type {type.FullName} must be decorated with a {typeof(VersionAttribute).FullName} attribute to denote the message version");
            }

            var forwardedType = type
                .FindInterfaceThatCloses(typeof(IForwardsTo<>))
                .GetGenericArguments()
                .Single();

            _forwarders[forwardedType].Add(type);
        }

        public IReadOnlyList<Type> ForwardingTypesTo(Type handledType)
        {
            return _forwarders.Has(handledType) ? _forwarders[handledType] : new List<Type>();
        }
    }


    public class ConnegDiscoveryFeature : IFeature
    {
        public void Dispose()
        {
            // nothing
        }

        public Task<ServiceRegistry> Bootstrap(JasperRegistry registry)
        {
            var forwarding = new Forwarders();
            var services = new ServiceRegistry();

            services.AddSingleton(forwarding);

            if (registry.ApplicationAssembly == null) return Task.FromResult(services);


            services.Scan(_ =>
            {
                _.Assembly(registry.ApplicationAssembly);
                _.AddAllTypesOf<IMessageSerializer>();
                _.AddAllTypesOf<IMessageDeserializer>();
            });

            return TypeRepository.FindTypes(registry.ApplicationAssembly,
                    TypeClassification.Closed, t => t.Closes(typeof(IForwardsTo<>)))
                .ContinueWith(t =>
                {
                    foreach (var type in t.Result)
                    {
                        forwarding.Add(type);
                    }

                    return services;
                });
        }

        public Task Activate(JasperRuntime runtime, GenerationRules generation)
        {
            return Task.CompletedTask;
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            // nothing
        }
    }
}
