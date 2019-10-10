using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jasper
{
    public static class OptionsExtensions
    {

        public static string ConfigSectionName(this Type type)
        {
            if (type.Name.EndsWith("Settings")) return type.Name.Substring(0, type.Name.Length - 8);
            if (type.Name.EndsWith("Options"))return type.Name.Substring(0, type.Name.Length - 7);

            return type.Name;
        }
    }
}
