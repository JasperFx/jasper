using System;
using System.Buffers;
using System.Linq;
using Baseline;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar
{
    public class InvalidPulsarUriException : Exception
    {
        public InvalidPulsarUriException(Uri actualUri) : base($"Invalid Jasper Pulsar Uri '{actualUri.ToString()}'. Should be of form 'pulsar://persistent/non-persistent/tenant/namespace/topic'")
        {
        }
    }

    public class PulsarEndpoint : Endpoint, IDisposable
    {
        private readonly PulsarTransport _parent;
        private PulsarListener _listener;
        public const string Persistent = "persistent";
        public const string NonPersistent = "non-persistent";
        public const string DefaultNamespace = "tenant";
        public const string Public = "public";

        public static Uri UriFor(bool persistent, string tenant, string @namespace, string topicName)
        {
            var scheme = persistent ? "persistent" : "non-persistent";
            return new Uri($"{scheme}://{tenant}/{@namespace}/{topicName}");
        }

        public static Uri UriFor(string topicPath)
        {
            var uri = new Uri(topicPath);
            return new Uri($"pulsar://{uri.Scheme}/{uri.Host}/{uri.Segments.Skip(1).Select(x => x.TrimEnd('/')).Join("/")}");
        }

        public PulsarEndpoint(Uri uri, PulsarTransport parent) : base(uri)
        {
            _parent = parent;
            Uri = uri;
        }

        public override Uri Uri { get; }
        public override Uri ReplyUri()
        {
            return Uri;
        }

        public string Persistence { get; private set; } = Persistent;
        public string Tenant { get; private set; } = Public;
        public string Namespace { get; private set; } = DefaultNamespace;
        public string TopicName { get; private set;}

        public IPulsarProtocol Protocol { get; set; } = new DefaultPulsarProtocol();

        public override void Parse(Uri uri)
        {
            if (uri.Segments.Length != 4)
            {
                throw new InvalidPulsarUriException(uri);
            }

            if (uri.Host != Persistent && uri.Host != NonPersistent)
            {
                throw new InvalidPulsarUriException(uri);
            }

            Persistence = uri.Host;
            Tenant = uri.Segments[1].TrimEnd('/');
            Namespace = uri.Segments[2].TrimEnd('/');
            TopicName = uri.Segments[3].TrimEnd('/');
        }

        public string PulsarTopic()
        {
            return $"{Persistence}://{Tenant}/{Namespace}/{TopicName}";
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            // TODO -- parallel listener option????

            _listener = new PulsarListener(this, _parent, root.Cancellation);

            runtime.AddListener(_listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new PulsarSender(this, _parent, root.Cancellation);
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
    }
}