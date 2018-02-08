using System;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Util;

namespace Jasper.Http
{
    internal class ActionMethodFilter : CompositeFilter<MethodInfo>
    {
        public static ActionMethodFilter Flyweight = new ActionMethodFilter();

        public ActionMethodFilter()
        {
            Excludes += method => method.DeclaringType == typeof(object);
            Excludes += method => method.Name == ReflectionHelper.GetMethod<IDisposable>(x => x.Dispose()).Name;
            Excludes += method => method.ContainsGenericParameters;
            Excludes += method => method.IsSpecialName;
        }

        public void IgnoreMethodsDeclaredBy<T>()
        {
            Excludes += x => x.DeclaringType == typeof(T);
        }
    }
}
