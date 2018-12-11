using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Transports;

namespace Jasper.Util
{
    public static class UriExtensions
    {
        private static readonly HashSet<string> _locals =
            new HashSet<string>(new[] {"localhost", "127.0.0.1"}, StringComparer.OrdinalIgnoreCase);

        public static string QueueName(this Uri uri)
        {
            if (uri == null) return null;

            if (uri.Scheme == TransportConstants.Loopback && uri.Host != TransportConstants.Durable) return uri.Host;

            var lastSegment = uri.Segments.Skip(1).LastOrDefault();
            if (lastSegment == TransportConstants.Durable) return TransportConstants.Default;

            return lastSegment ?? TransportConstants.Default;
        }

        public static bool IsDurable(this Uri uri)
        {
            if (uri.Scheme == TransportConstants.Loopback && uri.Host == TransportConstants.Durable) return true;

            var firstSegment = uri.Segments.Skip(1).FirstOrDefault();
            if (firstSegment == null) return false;

            return TransportConstants.Durable == firstSegment.TrimEnd('/');
        }


        public static Uri ToCanonicalTcpUri(this Uri uri)
        {
            if (uri.Scheme != TransportConstants.Durable)
                throw new ArgumentOutOfRangeException(nameof(uri),
                    "This only applies to Uri's with the scheme 'durable'");

            var queueName = uri.QueueName();

            return queueName == TransportConstants.Default
                ? $"tcp://{uri.Host}:{uri.Port}/{TransportConstants.Durable}".ToUri()
                : $"tcp://{uri.Host}:{uri.Port}/{TransportConstants.Durable}/{queueName}".ToUri();
        }

        public static Uri ToCanonicalUri(this Uri uri)
        {
            switch (uri.Scheme)
            {
                case "tcp":
                    return uri.IsDurable()
                        ? $"tcp://{uri.Host}:{uri.Port}/{TransportConstants.Durable}".ToUri()
                        : $"tcp://{uri.Host}:{uri.Port}".ToUri();

                case "durable":
                    return $"tcp://{uri.Host}:{uri.Port}/{TransportConstants.Durable}".ToUri();

                default:
                    return uri;
            }
        }
    }
}
