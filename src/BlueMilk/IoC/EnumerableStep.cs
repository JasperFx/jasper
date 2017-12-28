using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Internals.Codegen;

namespace Jasper.Internals.IoC
{
    public class EnumerableStep : BuildStep
    {
        private static readonly List<Type> _enumerableTypes = new List<Type>
        {
            typeof (IEnumerable<>),
            typeof (IList<>),
            typeof (IReadOnlyList<>),
            typeof (List<>)
        };

        private readonly BuildStep[] _childSteps;

        public static bool IsEnumerable(Type type)
        {
            if (type.IsArray) return true;

            return type.IsGenericType && _enumerableTypes.Contains(type.GetGenericTypeDefinition());
        }

        public static Type DetermineElementType(Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return serviceType.GetElementType();
            }

            return serviceType.GetGenericArguments().First();
        }

        public EnumerableStep(Type serviceType, BuildStep[] childSteps) : base(serviceType, false, false)
        {
            _childSteps = childSteps;
        }

        public override IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner)
        {
            return _childSteps.SelectMany(x => x.ReadDependencies(planner));
        }

        protected override Variable buildVariable()
        {
            var elements = _childSteps.Select(x => x.Variable).ToArray();
            return ServiceType.IsArray
                ? new ArrayAssignmentFrame(DetermineElementType(ServiceType), elements).Variable
                : new ListAssignmentFrame(ServiceType, elements).Variable;
        }
    }
}
