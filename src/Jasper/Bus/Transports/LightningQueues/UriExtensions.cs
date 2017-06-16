using System;
using System.Collections.Generic;

namespace Jasper.Bus.Transports.LightningQueues
{
    public static class UriExtensions
    {
        private static readonly HashSet<string> _locals = new HashSet<string>(new[]{"localhost", "127.0.0.1"}, StringComparer.OrdinalIgnoreCase);

        public static LightningUri ToLightningUri(this Uri uri)
        {
            return new LightningUri(uri);
        }

        public static LightningUri ToLightningUri(this string uri)
        {
            return new LightningUri(uri);
        }

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