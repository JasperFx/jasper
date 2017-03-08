using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jasper.Util
{
    public static class TypeRegistrationExtensions
    {
        // TODO -- move to Baseline
        public static IEnumerable<MethodInfo> PublicInstanceMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }
    }
}