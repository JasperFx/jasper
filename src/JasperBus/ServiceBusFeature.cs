using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Util;
using JasperBus.Model;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;

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

    public class HandlerSource
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();

        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();

        public HandlerSource()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.HasAttribute<NotHandlerAttribute>();
        }


        internal async Task<HandlerCall[]> FindCalls(IJasperRegistry registry)
        {
            if (registry.ApplicationAssembly == null) return new HandlerCall[0];


            // TODO -- need to expose the module assemblies off of this

            var types = await TypeRepository.FindTypes(registry.ApplicationAssembly,
                TypeClassification.Concretes | TypeClassification.Closed, _typeFilters.Matches);


            return types.SelectMany(actionsFromType).ToArray();
        }

        private IEnumerable<HandlerCall> actionsFromType(Type type)
        {
            return type.PublicInstanceMethods()
                .Where(_methodFilters.Matches)
                .Where(HandlerCall.IsCandidate)
                .Select(m => buildHandler(type, m));
        }

        protected virtual HandlerCall buildHandler(Type type, MethodInfo method)
        {
            return new HandlerCall(type, method);
        }

        /// <summary>
        /// Find Handlers on classes whose name ends on 'Consumer'
        /// </summary>
        public void IncludeClassesSuffixedWithConsumer()
        {
            IncludeClassesSuffixedWith("Consumer");
        }

        /// <summary>
        /// Find Handlers from classes whose name ends with 'Consumer'
        /// </summary>
        public void IncludeClassesSuffixedWithHandler()
        {
            IncludeClassesSuffixedWith("Handler");
        }

        /// <summary>
        /// Find Handlers from concrete classes whose names ends with the suffix
        /// </summary>
        /// <param name="suffix"></param>
        public void IncludeClassesSuffixedWith(string suffix)
        {
            IncludeTypesNamed(x => x.EndsWith(suffix));
        }

        public void IncludeTypesNamed(Func<string, bool> filter)
        {
            IncludeTypes(type => filter(type.Name));
        }

        /// <summary>
        /// Find Handlers on types that match on the provided filter
        /// </summary>
        public void IncludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Includes += filter;
        }

        /// <summary>
        /// Find Handlers on concrete types assignable to T
        /// </summary>
        public void IncludeTypesImplementing<T>()
        {
            IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
        }

        /// <summary>
        /// Handlers that match on the provided filter will be added to the runtime.
        /// </summary>
        public void IncludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Includes += filter;
        }



        /// <summary>
        /// Exclude types that match on the provided filter for finding Handlers
        /// </summary>
        public void ExcludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Excludes += filter;
        }

        /// <summary>
        /// Handlers that match on the provided filter will NOT be added to the runtime.
        /// </summary>
        public void ExcludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Excludes += filter;
        }

        /// <summary>
        /// Ignore any methods that are declared by a super type or interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void IgnoreMethodsDeclaredBy<T>()
        {
            _methodFilters.IgnoreMethodsDeclaredBy<T>();
        }

        /// <summary>
        /// Exclude any types that are not concrete
        /// </summary>
        public void ExcludeNonConcreteTypes()
        {
            _typeFilters.Excludes += type => !type.IsConcrete();
        }
    }
}