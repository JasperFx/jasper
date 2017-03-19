using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using StructureMap.TypeRules;

namespace JasperBus.Transports.LightningQueues
{
    public class LightningQueuesTransport
    {

    }

    public class LightningUri
    {
        public static readonly string Protocol = "lq.tcp";

        public LightningUri(string uriString) : this(new Uri(uriString))
        {

        }

        public LightningUri(Uri address)
        {
            if (address.Scheme != Protocol)
            {
                throw new ArgumentOutOfRangeException(
                    $"{address.Scheme} is the wrong protocol for a LightningQueue Uri.  Only {Protocol} is accepted");
            }

            Address = address.ToMachineUri();
            Port = address.Port;
            Original = address;
            QueueName = Address.Segments.Last();
        }

        public Uri Address { get; }

        public int Port { get; }

        public string QueueName { get; }
        public Uri Original { get; }
    }

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