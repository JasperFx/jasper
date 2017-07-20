using System;
using System.Linq;
using System.Reflection;

namespace Jasper
{
    internal class CallingAssembly
    {
        internal static Assembly Find()
        {
            string trace = Environment.StackTrace;



            var parts = trace.Split('\n');

            for (int i = 4; i < parts.Length; i++)
            {
                var line = parts[i];
                var assembly = findAssembly(line);
                if (assembly != null && !isSystemAssembly(assembly))
                {
                    return assembly;
                }
            }

            return null;
        }

        private static bool isSystemAssembly(Assembly assembly)
        {
            if (assembly == null) return false;

            if (assembly.GetCustomAttributes<JasperFeatureAttribute>().Any()) return true;

            if (assembly.GetName().Name == "Jasper") return true;

            return assembly.GetName().Name.StartsWith("System.");
        }

        private static Assembly findAssembly(string stacktraceLine)
        {
            var candidate = stacktraceLine.Trim().Substring(3);

            // Short circuit this
            if (candidate.StartsWith("System.")) return null;

            Assembly assembly = null;
            var names = candidate.Split('.');
            for (var i = names.Length - 2; i > 0; i--)
            {
                var possibility = string.Join(".", names.Take(i).ToArray());

                try
                {

                    assembly = System.Reflection.Assembly.Load(new AssemblyName(possibility));
                    break;
                }
                catch
                {
                    // Nothing
                }
            }

            return assembly;
        }
    }
}
