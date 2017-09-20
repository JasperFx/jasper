using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using Jasper.Http.Routing;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using TypeExtensions = Baseline.TypeExtensions;

namespace Jasper.Http
{
    public class ActionSource
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly CompositeFilter<MethodCall> _callFilters = new CompositeFilter<MethodCall>();

        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();

        public ActionSource()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.Name == "Configure";
        }

        public AppliesToExpression Applies { get; } = new AppliesToExpression();

        public static bool IsCandidate(MethodInfo method)
        {
            if (method.DeclaringType == typeof(object)) return false;

            if (method.HasAttribute<JasperIgnoreAttribute>()) return false;
            if (method.DeclaringType.HasAttribute<JasperIgnoreAttribute>()) return false;

            return HttpVerbs.All.Any(x => method.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase));
        }


        internal Task<MethodCall[]> FindActions(Assembly applicationAssembly)
        {
            if (applicationAssembly == null)
            {
                return Task.FromResult(new MethodCall[0]);
            }

            var assemblies = Applies.Assemblies.Any() ? Applies.Assemblies : new[] {applicationAssembly};
            return TypeRepository.FindTypes(assemblies, TypeClassification.Concretes, _typeFilters.Matches)
                .ContinueWith(x => x.Result.SelectMany(actionsFromType).ToArray());
        }

        private IEnumerable<MethodCall> actionsFromType(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Where(_methodFilters.Matches)
                .Where(IsCandidate)
                .Select(m => buildAction(type, m))
                .Where(_callFilters.Matches);
        }

        protected virtual MethodCall buildAction(Type type, MethodInfo method)
        {
            return new MethodCall(type, method);
        }

        /// <summary>
        ///     Find Actions on classes whose name ends on 'Controller'
        /// </summary>
        public void IncludeClassesSuffixedWithController()
        {
            IncludeTypesNamed(x => x.EndsWith("Controller"));
        }

        /// <summary>
        ///     Find Actions on classes whose name ends with 'Endpoint'
        /// </summary>
        internal void IncludeClassesSuffixedWithEndpoint()
        {
            IncludeTypesNamed(
                x =>
                    x.EndsWith("Endpoint", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith("Endpoints", StringComparison.OrdinalIgnoreCase));
        }

        public void IncludeTypesNamed(Func<string, bool> filter)
        {
            IncludeTypes(type => filter(type.Name));
        }

        /// <summary>
        ///     Find Actions on types that match on the provided filter
        /// </summary>
        public void IncludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Includes += filter;
        }

        /// <summary>
        ///     Find Actions on concrete types assignable to T
        /// </summary>
        public void IncludeTypesImplementing<T>()
        {
            IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
        }

        /// <summary>
        ///     Actions that match on the provided filter will be added to the runtime.
        /// </summary>
        public void IncludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Includes += filter;
        }

        /// <summary>
        ///     Exclude types that match on the provided filter for finding Actions
        /// </summary>
        public void ExcludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Excludes += filter;
        }

        /// <summary>
        ///     Actions that match on the provided filter will NOT be added to the runtime.
        /// </summary>
        public void ExcludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Excludes += filter;
        }

        /// <summary>
        ///     Ignore any methods that are declared by a super type or interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void IgnoreMethodsDeclaredBy<T>()
        {
            _methodFilters.IgnoreMethodsDeclaredBy<T>();
        }

        /// <summary>
        ///     Exclude any types that are not concrete
        /// </summary>
        public void ExcludeNonConcreteTypes()
        {
            _typeFilters.Excludes += type => !type.IsConcrete();
        }
    }
}
