using System;
using System.Collections.Generic;

namespace Jasper.Internals.Util
{
    /// <summary>
    /// Taken directly from Marten: https://github.com/JasperFx/marten/blob/2f18d09fa2034cbc647f48a74bbf3bbb8ea51116/src/Marten/Util/EnumerableExtensions.cs
    /// </summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = true)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                Visit(item, visited, sorted, dependencies, throwOnCycle);
            }

            return sorted;
        }

        private static void Visit<T>(T item, ISet<T> visited, ICollection<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
        {
            if (visited.Contains(item))
            {
                if (throwOnCycle && !sorted.Contains(item))
                {
                    throw new Exception("Cyclic dependency found");
                }
            }
            else
            {
                visited.Add(item);

                foreach (var dep in dependencies(item))
                {
                    Visit(dep, visited, sorted, dependencies, throwOnCycle);
                }

                sorted.Add(item);
            }
        }
    }
}
