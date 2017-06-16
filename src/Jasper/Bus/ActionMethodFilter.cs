using System;
using System.Reflection;
using StructureMap.Graph;

namespace Jasper.Bus
{
    // TODO -- might be smart to move this up to Jasper's core
    public class ActionMethodFilter : CompositeFilter<MethodInfo>
    {
        public ActionMethodFilter()
        {
            Excludes += method => method.DeclaringType == typeof(object);
            Excludes += method => method.Name == nameof(IDisposable.Dispose);
            Excludes += method => method.ContainsGenericParameters;
            Excludes += method => method.IsSpecialName;
        }

        public void IgnoreMethodsDeclaredBy<T>()
        {
            Excludes += x => x.DeclaringType == typeof (T);
        }
    }
}