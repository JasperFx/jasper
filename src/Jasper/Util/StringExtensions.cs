using System;
using System.Linq;
using Baseline;

namespace Jasper.Util
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string uriString)
        {
            if (uriString.Contains("://*"))
            {
                var parts = uriString.Split(':');

                var protocol = parts[0];
                var segments = parts[2].Split('/');
                var port = int.Parse(segments.First());

                var uri = $"{protocol}://localhost:{port}/{segments.Skip(1).Join("/")}";
                return new Uri(uri);
            }

            return uriString.IsEmpty() ? null : new Uri(uriString);
        }

        public static bool IsIn(this string text, params string[] values)
        {
            return values.Contains(text);
        }
    }
}
