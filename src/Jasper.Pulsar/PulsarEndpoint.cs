using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar
{
    public class PulsarEndpoint : TransportEndpoint<IMessage<ReadOnlySequence<byte>>, MessageMetadata>, IDisposable
    {
        private readonly PulsarTransport _parent;
        private PulsarListener? _listener;
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

        public override IDictionary<string, object> DescribeProperties()
        {
            var dict = base.DescribeProperties();

            dict.Add(nameof(Persistent), Persistent);
            dict.Add(nameof(Tenant), Tenant);
            dict.Add(nameof(Namespace), Namespace);
            if (TopicName != null)
            {
                dict.Add(nameof(TopicName), TopicName);
            }

            return dict;
        }

        public override Uri Uri { get; }
        public override Uri CorrectedUriForReplies()
        {
            return Uri;
        }

        public string Persistence { get; private set; } = Persistent;
        public string Tenant { get; private set; } = Public;
        public string Namespace { get; private set; } = DefaultNamespace;
        public string? TopicName { get; private set;}

        protected override void writeOutgoingHeader(MessageMetadata outgoing, string key, string value)
        {
            outgoing[key] = value;
        }

        protected override bool tryReadIncomingHeader(IMessage<ReadOnlySequence<byte>> incoming, string key,
            out string? value)
        {
            return incoming.Properties.TryGetValue(key, out value);
        }

        protected override void writeIncomingHeaders(IMessage<ReadOnlySequence<byte>> incoming, Envelope envelope)
        {
            foreach (var pair in incoming.Properties)
            {
                envelope.Headers[pair.Key] = pair.Value;
            }
        }

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

        public override void StartListening(IJasperRuntime runtime)
        {
            if (!IsListener) return;

            // TODO -- parallel listener option????

            _listener = new PulsarListener(this, _parent, runtime.Cancellation);

            runtime.Endpoints.AddListener(_listener, this);
        }

        protected override ISender CreateSender(IJasperRuntime root)
        {
            return new PulsarSender(this, _parent, root.Cancellation);
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
    }
}
