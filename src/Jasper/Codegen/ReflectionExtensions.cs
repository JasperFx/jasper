using System.Reflection;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Codegen
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
    }
}