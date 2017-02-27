using System;
using System.Linq;
using System.Reflection;
using Baseline;
using JasperBus.Runtime;

namespace JasperBus.Model
{
    public static class MethodInfoExtensions
    {
        public static Type MessageType(this MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (!parameters.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(method), $"Method {method.DeclaringType.FullName}.{method.Name} has no parameters");
            }

            if (parameters.Length == 1)
            {
                var type = parameters.First().ParameterType;
                if (type.IsConcrete()) return type;
            }

            var first = parameters.FirstOrDefault(x => x.Name.IsIn("message", "input"));

            if (first != null) return first.ParameterType;

            throw new ArgumentOutOfRangeException(nameof(method), $"Could not determine a message type for {method.DeclaringType.FullName}.{method.Name}");

        }
    }
}