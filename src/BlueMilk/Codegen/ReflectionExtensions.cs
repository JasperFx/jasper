using System;
using System.Reflection;
using System.Threading.Tasks;
using BlueMilk.Util;

namespace BlueMilk.Codegen
{
    public static class ReflectionExtensions
    {
        public static bool IsAsync(this MethodInfo method)
        {
            if (method.ReturnType == null)
            {
                return false;
            }

            return method.ReturnType == typeof(Task) || method.ReturnType.Closes(typeof(Task<>));

        }

        public static string FullNameInCode(this Type type)
        {
            return type.FullName.Replace("+", ".");
        }

        public static string NameInCode(this Type type)
        {
            return type.Name.Replace("+", ".");
        }
    }
}
