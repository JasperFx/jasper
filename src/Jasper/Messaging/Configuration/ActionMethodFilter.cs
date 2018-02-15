using System;
using System.Reflection;
using Jasper.Util;

namespace Jasper.Messaging.Configuration
{
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
            Excludes += x => x.DeclaringType == typeof(T);
        }
    }
}
