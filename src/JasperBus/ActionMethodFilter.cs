using System;
using System.Linq;
using System.Reflection;
using StructureMap.Graph;
using TypeExtensions = Baseline.TypeExtensions;

namespace JasperBus
{
    // TODO -- might be smart to move this up to Jasper's core
    public class ActionMethodFilter : CompositeFilter<MethodInfo>
    {
        public ActionMethodFilter()
        {
            Excludes += method => method.DeclaringType == typeof(object);
            Excludes += method => method.Name == nameof(IDisposable.Dispose);
            Excludes += method => method.ContainsGenericParameters;
            Excludes += method => method.GetParameters().Any(x => TypeExtensions.IsSimple(x.ParameterType));
            Excludes += method => method.IsSpecialName;
        }

        public void IgnoreMethodsDeclaredBy<T>()
        {
            Excludes += x => x.DeclaringType == typeof (T);
        }
    }
}