using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen;
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

            var parameterCount = method.GetParameters() == null ? 0 : method.GetParameters().Length;
            if (parameterCount > 1) return false;

            if (method.GetParameters().Any(x => x.ParameterType.IsSimple())) return false;

            var hasOutput = method.ReturnType != typeof(void);

            if (hasOutput && method.ReturnType == typeof(int)) return true;

            if (hasOutput && !method.ReturnType.GetTypeInfo().IsClass) return false;


            if (hasOutput) return true;

            return parameterCount == 1;
        }


        internal Task<MethodCall[]> FindActions(Assembly applicationAssembly)
        {
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
