using System;
using System.Linq;
using Baseline;

namespace Jasper.Bus.Runtime
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string uriString)
        {
            return uriString.IsEmpty() ? null : new Uri(uriString);
        }

        public static bool IsIn(this string text, params string[] values)
        {
            return values.Contains(text);
        }
    }
}