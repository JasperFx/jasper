using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Http.Routing;
using Jasper.Util;
using Lamar.Codegen.Frames;
using Lamar.Scanning;

namespace Jasper.Http
{
    public partial class HttpSettings
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly CompositeFilter<MethodCall> _callFilters = new CompositeFilter<MethodCall>();
        private readonly IList<Type> _explicitTypes = new List<Type>();

        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();
        private bool _disableConventionalDiscovery;

        public AppliesToExpression Applies { get; } = new AppliesToExpression();

        public static bool IsCandidate(MethodInfo method)
        {
            if (method.DeclaringType == typeof(object)) return false;

            if (method.HasAttribute<JasperIgnoreAttribute>()) return false;
            if (method.DeclaringType.HasAttribute<JasperIgnoreAttribute>()) return false;

            if (method.Name.EqualsIgnoreCase("Index")) return true;

            return HttpVerbs.All.Contains(method.Name, StringComparer.OrdinalIgnoreCase) || HttpVerbs.All.Any(x => method.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase));
        }


        internal async Task<MethodCall[]> FindActions(Assembly applicationAssembly)
        {
            if (applicationAssembly == null || _disableConventionalDiscovery)
                return _explicitTypes.SelectMany(actionsFromType).ToArray();

            var assemblies = Applies.Assemblies.Any() ? Applies.Assemblies : new[] {applicationAssembly};

            var discovered =
                await TypeRepository.FindTypes(assemblies, TypeClassification.Concretes, _typeFilters.Matches);

            return discovered
                .Concat(_explicitTypes)
                .Distinct()
                .SelectMany(actionsFromType).ToArray();
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
        public HttpSettings IncludeClassesSuffixedWithController()
        {
            return IncludeTypesNamed(x => x.EndsWith("Controller"));
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

        public HttpSettings IncludeTypesNamed(Func<string, bool> filter)
        {
            return IncludeTypes(type => filter(type.Name));
        }

        /// <summary>
        ///     Find Actions on types that match on the provided filter
        /// </summary>
        public HttpSettings IncludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Includes += filter;
            return this;
        }

        /// <summary>
        ///     Find Actions on concrete types assignable to T
        /// </summary>
        public HttpSettings IncludeTypesImplementing<T>()
        {
            return IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
        }

        /// <summary>
        ///     Actions that match on the provided filter will be added to the runtime.
        /// </summary>
        public HttpSettings IncludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Includes += filter;
            return this;
        }

        /// <summary>
        ///     Exclude types that match on the provided filter for finding Actions
        /// </summary>
        public HttpSettings ExcludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Excludes += filter;
            return this;
        }

        /// <summary>
        ///     Actions that match on the provided filter will NOT be added to the runtime.
        /// </summary>
        public HttpSettings ExcludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Excludes += filter;
            return this;
        }

        /// <summary>
        ///     Ignore any methods that are declared by a super type or interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public HttpSettings IgnoreMethodsDeclaredBy<T>()
        {
            _methodFilters.IgnoreMethodsDeclaredBy<T>();
            return this;
        }

        /// <summary>
        ///     Exclude any types that are not concrete
        /// </summary>
        public HttpSettings ExcludeNonConcreteTypes()
        {
            _typeFilters.Excludes += type => !type.IsConcrete();
            return this;
        }

        /// <summary>
        ///     Explicitly add this type as a candidate for HTTP endpoints
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public HttpSettings IncludeType<T>()
        {
            return IncludeType(typeof(T));
        }

        /// <summary>
        ///     Explicitly add this type as a candidate for HTTP endpoints
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public HttpSettings IncludeType(Type type)
        {
            _explicitTypes.Fill(type);
            return this;
        }

        /// <summary>
        ///     Disables explicit discovery of HTTP endpoints
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public HttpSettings DisableConventionalDiscovery()
        {
            _disableConventionalDiscovery = true;
            return this;
        }
    }
}
