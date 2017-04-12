using System;
using System.Linq;
using System.Reflection;
using Baseline.Reflection;
using StructureMap.Graph;
using TypeExtensions = Baseline.TypeExtensions;

namespace JasperHttp
{
    internal class ActionMethodFilter : CompositeFilter<MethodInfo>
    {
        public static ActionMethodFilter Flyweight = new ActionMethodFilter();

        public ActionMethodFilter()
        {
            Excludes += method => method.DeclaringType == typeof(object);
            Excludes += method => method.Name == ReflectionHelper.GetMethod<IDisposable>(x => x.Dispose()).Name;
            Excludes += method => method.ContainsGenericParameters;
            Excludes += method => method.GetParameters().Any(x => TypeExtensions.IsSimple(x.ParameterType));
            Excludes += method => method.IsSpecialName;
        }

        public void IgnoreMethodsDeclaredBy<T>()
        {
            Excludes += x => x.DeclaringType == typeof(T);
        }
    }
}