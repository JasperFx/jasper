using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Codegen;
using Jasper.Configuration;
using StructureMap;
using StructureMap.Graph.Scanning;
using TypeClassification = Jasper.Util.TypeClassification;
using TypeRepository = Jasper.Util.TypeRepository;

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

        public Task<Registry> Bootstrap(JasperRegistry registry)
        {
            var forwarding = new Forwarders();
            var services = new Registry();
            services.For<Forwarders>().Use(forwarding);

            if (registry.ApplicationAssembly == null) return Task.FromResult(services);

            return TypeRepository.FindTypes(registry.ApplicationAssembly,
                    TypeClassification.Closed | TypeClassification.Closed, t => t.Closes(typeof(IForwardsTo<>)))
                .ContinueWith(t =>
                {
                    foreach (var type in t.Result)
                    {
                        forwarding.Add(type);
                    }

                    return services;
                });
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.CompletedTask;
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            // nothing
        }
    }
}
