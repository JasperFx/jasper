using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;

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
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First().Replace("+", ".");
                var args = type.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ");

                return $"{type.Namespace}.{cleanName}<{args}>";
            }

            return type.FullName.Replace("+", ".");
        }

        public static string NameInCode(this Type type)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First().Replace("+", ".");
                var args = type.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ");

                return $"{cleanName}<{args}>";
            }

            return type.Name.Replace("+", ".");
        }


    }
}
