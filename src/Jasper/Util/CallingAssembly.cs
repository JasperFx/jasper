﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Configuration;

namespace Jasper.Util
{
    [Obsolete("Replace with Lamar equivalent")]
    internal class CallingAssembly
    {
        private static readonly IList<string> _misses = new List<string>();

        internal static Assembly Find()
        {
            var trace = Environment.StackTrace;


            var parts = trace.Split('\n');

            for (var i = 5; i < parts.Length; i++)
            {
                var line = parts[i];
                var assembly = findAssembly(line);
                if (assembly != null && !isSystemAssembly(assembly)) return assembly;
            }

            return null;
        }

        private static bool isSystemAssembly(Assembly assembly)
        {
            if (assembly == null) return false;

            if (assembly.GetCustomAttributes<JasperFeatureAttribute>().Any()) return true;

            if (assembly.GetName().Name == "Jasper") return true;
            if (assembly.GetName().Name == "Jasper.CommandLine") return true;
            if (assembly.GetName().Name == "Jasper.Http") return true;


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

                if (_misses.Contains(possibility)) continue;

                try
                {
                    assembly = Assembly.Load(new AssemblyName(possibility));
                    break;
                }
                catch
                {
                    _misses.Add(possibility);
                }
            }

            return assembly;
        }

        public static Assembly DetermineApplicationAssembly(JasperRegistry registry)
        {
            var assembly = registry.GetType().Assembly;
            var isFeature = assembly.GetCustomAttribute<JasperFeatureAttribute>() != null;
            if (!Equals(assembly, typeof(JasperRegistry).Assembly) && !isFeature)
                return assembly;
            return Find();
        }
    }
}
