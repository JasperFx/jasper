using Microsoft.AspNetCore.Http;

namespace Jasper.Diagnostics
{
    internal static class StringExtensions
    {
        public static string CombineUrl(this PathString url, params string[] urls)
        {
            return CombineUrl(url.ToString(), urls);
        }

        public static string CombineUrl(this string url, params string[] urls)
        {
            var start = url.ToString();

            foreach(var u in urls)
            {
                start = $"{start.TrimEnd('/', '\\')}/{(u ?? "").TrimStart('/', '\\')}";
            }

            return start;
        }
    }
}
