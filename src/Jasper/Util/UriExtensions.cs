using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Loopback;

namespace Jasper.Util
{
    public static class UriExtensions
    {
        public static string QueueName(this Uri uri)
        {
            if (uri == null) return null;

            if (uri.Scheme == LoopbackTransport.ProtocolName)
            {
                return uri.Host;
            }

            return uri.Segments.Skip(1).LastOrDefault() ?? TransportConstants.Default;
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
