using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Configuration;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Microsoft.Extensions.DependencyInjection;
using TypeClassification = Jasper.Internals.Scanning.TypeClassification;
using TypeRepository = Jasper.Internals.Scanning.TypeRepository;

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
            return _forwarders[handledType];
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

            // TODO -- move serializer discovery here

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
