using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper;
using Jasper.Util;
using JasperBus.Model;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;

namespace JasperBus
{
    public class HandlerSource
    {
        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();

        public HandlerSource()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.HasAttribute<NotHandlerAttribute>();

            IncludeClassesSuffixedWith("Handler");
            IncludeClassesSuffixedWith("Consumer");

        }


        internal async Task<HandlerCall[]> FindCalls(IJasperRegistry registry)
        {
            if (registry.ApplicationAssembly == null) return new HandlerCall[0];


            // TODO -- need to expose the module assemblies off of this

            var types = await TypeRepository.FindTypes(registry.ApplicationAssembly,
                TypeClassification.Concretes | TypeClassification.Closed, type => _typeFilters.Matches(type))
                .ConfigureAwait(false);


            return types.Where(x => !x.HasAttribute<NotHandlerAttribute>()).SelectMany(actionsFromType).ToArray();
        }

        private IEnumerable<HandlerCall> actionsFromType(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Where(_methodFilters.Matches)
                .Where(HandlerCall.IsCandidate)
                .Select(m => buildHandler(type, m));
        }

        protected virtual HandlerCall buildHandler(Type type, MethodInfo method)
        {
            return new HandlerCall(type, method);
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