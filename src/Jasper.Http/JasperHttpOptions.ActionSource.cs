using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using BaselineTypeDiscovery;
using Jasper.Util;
using LamarCodeGeneration.Frames;

namespace Jasper.Http
{
    public partial class JasperHttpOptions
    {
        private readonly CompositeFilter<MethodCall> _callFilters = new CompositeFilter<MethodCall>();
        private readonly IList<Type> _explicitTypes = new List<Type>();

        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();
        private bool _disableConventionalDiscovery;

        public AppliesToExpression Applies { get; } = new AppliesToExpression();

        /// <summary>
        ///     Controls which .Net methods are considered to be HTTP action methods
        /// </summary>
        public CompositeFilter<MethodInfo> MethodFilters => _methodFilters;

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
        public JasperHttpOptions IncludeClassesSuffixedWithController()
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

        public JasperHttpOptions IncludeTypesNamed(Func<string, bool> filter)
        {
            return IncludeTypes(type => filter(type.Name));
        }

        /// <summary>
        ///     Find Actions on types that match on the provided filter
        /// </summary>
        public JasperHttpOptions IncludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Includes += filter;
            return this;
        }

        /// <summary>
        ///     Find Actions on concrete types assignable to T
        /// </summary>
        public JasperHttpOptions IncludeTypesImplementing<T>()
        {
            return IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
        }

        /// <summary>
        ///     Actions that match on the provided filter will be added to the runtime.
        /// </summary>
        public JasperHttpOptions IncludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Includes += filter;
            return this;
        }

        /// <summary>
        ///     Exclude types that match on the provided filter for finding Actions
        /// </summary>
        public JasperHttpOptions ExcludeTypes(Func<Type, bool> filter)
        {
            _typeFilters.Excludes += filter;
            return this;
        }

        /// <summary>
        ///     Actions that match on the provided filter will NOT be added to the runtime.
        /// </summary>
        public JasperHttpOptions ExcludeMethods(Func<MethodInfo, bool> filter)
        {
            _methodFilters.Excludes += filter;
            return this;
        }

        /// <summary>
        ///     Ignore any methods that are declared by a super type or interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public JasperHttpOptions IgnoreMethodsDeclaredBy<T>()
        {
            _methodFilters.IgnoreMethodsDeclaredBy<T>();
            return this;
        }

        /// <summary>
        ///     Exclude any types that are not concrete
        /// </summary>
        public JasperHttpOptions ExcludeNonConcreteTypes()
        {
            _typeFilters.Excludes += type => !type.IsConcrete();
            return this;
        }

        /// <summary>
        ///     Explicitly add this type as a candidate for HTTP endpoints
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public JasperHttpOptions IncludeType<T>()
        {
            return IncludeType(typeof(T));
        }

        /// <summary>
        ///     Explicitly add this type as a candidate for HTTP endpoints
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public JasperHttpOptions IncludeType(Type type)
        {
            _explicitTypes.Fill(type);
            return this;
        }

        /// <summary>
        ///     Disables explicit discovery of HTTP endpoints
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public JasperHttpOptions DisableConventionalDiscovery()
        {
            _disableConventionalDiscovery = true;
            return this;
        }
    }
}
