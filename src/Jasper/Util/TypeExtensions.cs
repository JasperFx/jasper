using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Jasper.Util
{
    public static class TypeExtensions
    {
        private static readonly Regex _aliasSanitizer = new Regex("<|>", RegexOptions.Compiled);

        public static string GetPrettyName(this Type t)
        {
            if (!t.GetTypeInfo().IsGenericType)
                return t.Name;

            var sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.Ordinal)));
            sb.Append(t.GetGenericArguments().Aggregate("<", (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetPrettyName(type)));
            sb.Append(">");

            return sb.ToString();
        }

        public static string ToTypeAlias(this Type type)
        {
            var nameToAlias = type.Name;
            if (type.GetTypeInfo().IsGenericType)
            {
                nameToAlias = _aliasSanitizer.Replace(type.GetPrettyName(), string.Empty);
            }
            var parts = new List<string> {nameToAlias.ToLower()};
            if (type.IsNested)
            {
                parts.Insert(0, type.DeclaringType.Name.ToLower());
            }

            return string.Join("_", parts);
        }
    }
}
