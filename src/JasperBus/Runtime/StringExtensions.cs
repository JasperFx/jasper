using System;
using Baseline;

namespace JasperBus.Runtime
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string uriString)
        {
            return uriString.IsEmpty() ? null : new Uri(uriString);
        }
    }
}