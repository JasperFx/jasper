using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Util
{
    public static class UriExtensions
    {
        public static string QueueName(this Uri uri)
        {
            return uri.Segments.Last();
        }

        private static readonly HashSet<string> _locals = new HashSet<string>(new[] { "localhost", "127.0.0.1" }, StringComparer.OrdinalIgnoreCase);

        public static Uri ToMachineUri(this Uri uri)
        {
            return _locals.Contains(uri.Host) ? uri.ToLocalUri() : uri;
        }

        public static Uri ToLocalUri(this Uri uri)
        {
            return new UriBuilder(uri) { Host = Environment.MachineName }.Uri;
        }
    }
}
