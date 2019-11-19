using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.Configuration;

namespace Jasper.Util
{
    public static class UriExtensions
    {

        [Obsolete("Think this needs to be done in LocalTransport")]
        public static Uri AtQueue(this Uri uri, string queueName)
        {
            if (queueName.IsEmpty()) return uri;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable)
            {
                return new Uri("local://" + queueName);
            }

            return new Uri(uri, queueName);
        }

        [Obsolete("Think this needs to be done in LocalTransport")]
        public static string QueueName(this Uri uri)
        {
            if (uri == null) return null;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable) return uri.Host;

            var lastSegment = uri.Segments.Skip(1).LastOrDefault();
            if (lastSegment == TransportConstants.Durable) return TransportConstants.Default;

            return lastSegment ?? TransportConstants.Default;
        }

        [Obsolete("This needs to go away. Might hurt in Storyteller world")]
        public static bool IsDurable(this Uri uri)
        {
            if (uri.Scheme == TransportConstants.Local && uri.Host == TransportConstants.Durable) return true;

            var firstSegment = uri.Segments.Skip(1).FirstOrDefault();
            if (firstSegment == null) return false;

            return TransportConstants.Durable == firstSegment.TrimEnd('/');
        }

    }
}
