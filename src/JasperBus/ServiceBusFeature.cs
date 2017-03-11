using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperBus.Model;
using StructureMap;
using StructureMap.Graph;

namespace JasperBus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public void Dispose()
        {
            // shut down transports
        }

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime)
        {
            return Task.Factory.StartNew(() =>
            {
                _graph.CompileAndBuildAll(runtime.Container);

                // TODO
                // 1. Start up transports
                // 2. Start up subscriptions when ready

            });


        }

        private async Task<Registry> bootstrap(IJasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            // TODO -- configure the generation config?
            // TODO -- let it vary between features?
            var generationConfig = new GenerationConfig(registry.ApplicationAssembly.GetName().Name + ".JasperGenerated");
            _graph = new HandlerGraph(generationConfig);
            _graph.AddRange(calls);

            

            // TODO -- this will probably be a custom Registry later
            var services = new Registry();
            services.For<HandlerGraph>().Use(_graph);

            return services;
        }
    }

    public class ActionMethodFilter : CompositeFilter<MethodInfo>
    {
        public ActionMethodFilter()
        {
            Excludes += method => method.DeclaringType == typeof(object);
            Excludes += method => method.Name == nameof(IDisposable.Dispose);
            Excludes += method => method.ContainsGenericParameters;
            Excludes += method => method.GetParameters().Any(x => x.ParameterType.IsSimple());
            Excludes += method => method.IsSpecialName;
        }

        public void IgnoreMethodsDeclaredBy<T>()
        {
            Excludes += x => x.DeclaringType == typeof (T);
        }
    }
}