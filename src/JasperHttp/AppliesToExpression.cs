using System;
using System.Collections.Generic;
using System.Reflection;

namespace JasperHttp
{
    public class AppliesToExpression
    {
        private readonly IList<Assembly> _assemblies = new List<Assembly>();

        internal IEnumerable<Assembly> Assemblies => _assemblies;

        /// <summary>
        ///     Include the given assembly
        /// </summary>
        public void ToAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
        }

        /// <summary>
        ///     Include the assembly containing the provided type
        /// </summary>
        public void ToAssemblyContainingType<T>()
        {
            ToAssemblyContainingType(typeof(T));
        }

        /// <summary>
        ///     Include the assembly containing the provided type
        /// </summary>
        public void ToAssemblyContainingType(Type type)
        {
            ToAssembly(type.GetTypeInfo().Assembly);
        }

        /// <summary>
        ///     Include the assembly identified by the provided name.
        ///     All restrictions known from <see cref="Assembly.Load(string)" /> apply.
        /// </summary>
        public void ToAssembly(string assemblyName)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            ToAssembly(assembly);
        }
    }
}
