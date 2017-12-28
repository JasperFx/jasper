using System.Reflection;

namespace Jasper.Internals.Scanning.Conventions
{
    public static class AssemblyLoader
    {
        public static Assembly ByName(string assemblyName)
        {
            return Assembly.Load(new AssemblyName(assemblyName));
        }
    }
}