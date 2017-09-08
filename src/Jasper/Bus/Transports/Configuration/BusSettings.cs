using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Durable;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Bus.Transports.Loopback;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Transports.Configuration
{
    public class BusSettings : ITransportsExpression
    {
        private readonly LightweightCache<string, TransportSettings> _transports = new LightweightCache<string, TransportSettings>();

        public BusSettings()
        {
            _transports[LightweightTransport.ProtocolName] = new TransportSettings(LightweightTransport.ProtocolName)
            {
                MaximumSendAttempts = 3
            };

            _transports[DurableTransport.ProtocolName] = new TransportSettings(DurableTransport.ProtocolName)
            {
                MaximumSendAttempts = 100
            };

            _transports[LoopbackTransport.ProtocolName]= new TransportSettings(LoopbackTransport.ProtocolName)
            {
                State = TransportState.Enabled
            };

            ListenForMessagesFrom(TransportConstants.RetryUri).MaximumParallelization(5);
            ListenForMessagesFrom(TransportConstants.DelayedUri).MaximumParallelization(5);
            ListenForMessagesFrom(TransportConstants.RepliesUri).MaximumParallelization(5);
        }

        // Was ChannelGraph.Name
        public string ServiceName { get; set; }

        public Uri DefaultChannelAddress
        {
            get => _defaultChannelAddress;
            set
            {
                if (value != null && value.Scheme == LoopbackTransport.ProtocolName)
                {
                    ListenForMessagesFrom(value);
                }

                _defaultChannelAddress = value;
            }
        }

        public TransportSettings this[string protocolName] => _transports[protocolName];

        public TransportSettings Lightweight => _transports[LightweightTransport.ProtocolName];

        public TransportSettings Durable => _transports[DurableTransport.ProtocolName];

        public TransportSettings Loopback => _transports[LoopbackTransport.ProtocolName];

        void ITransportsExpression.DefaultIs(string uriString)
        {
            DefaultChannelAddress = uriString.ToUri();
        }

        void ITransportsExpression.DefaultIs(Uri uri)
        {
            DefaultChannelAddress = uri;
        }

        ITransportExpression ITransportsExpression.Durable => Durable;

        ITransportExpression ITransportsExpression.Lightweight => Lightweight;

        ILoopbackTransportExpression ITransportsExpression.Loopback => Loopback;

        public bool DisableAllTransports { get; set; }

        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public bool ThrowOnValidationErrors { get; set; } = true;

        public bool AllowNonVersionedSerialization { get; set; } = true;

        public string MachineName { get; set; } = Environment.MachineName;



        // Catches anything from unknown transports
        public readonly IList<Listener> Listeners = new List<Listener>();


        public IQueueSettings ListenForMessagesFrom(Uri uri)
        {
            if (_transports.Has(uri.Scheme))
            {
                var transport = _transports[uri.Scheme];
                var port = uri.Port;
                if (port > 0)
                {
                    validatePort(uri, transport);

                    transport.Port = port;
                }

                var queueName = uri.QueueName();

                return transport.Queues[queueName];
            }
            else
            {
                var listener = Listeners.FirstOrDefault(x => x.Uri == uri) ?? new Listener(uri);
                Listeners.Fill(listener);

                return listener;
            }
        }

        public readonly IList<SubscriberAddress> KnownSubscribers = new List<SubscriberAddress>();
        private Uri _defaultChannelAddress = TransportConstants.RepliesUri;

        public SubscriberAddress SendTo(Uri uri)
        {
            var subscriber = KnownSubscribers.FirstOrDefault(x => x.Uri == uri) ?? new SubscriberAddress(uri);
            KnownSubscribers.Fill(subscriber);

            return subscriber;
        }

        public ISubscriberAddress SendTo(string uriString)
        {
            return SendTo(uriString.ToUri());
        }

        private void validatePort(Uri uri, TransportSettings transport)
        {
            if (transport.Port.HasValue && transport.Port.Value != uri.Port)
            {
                throw new InvalidOperationException($"Transport '{uri.Scheme}' is already listening on port {transport.Port.Value}");
            }


            var alreadyUsedTransport = _transports
                .GetAll().FirstOrDefault(x => x.Port.HasValue && x.Port.Value == uri.Port);

            if (alreadyUsedTransport != null && alreadyUsedTransport != transport)
            {
                throw new InvalidOperationException($"Port {uri.Port} is already in usage by registered transport '{alreadyUsedTransport.Protocol}'");
            }
        }

        public IQueueSettings ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

        public async Task ApplyLookups(UriAliasLookup lookups)
        {
            var all = Listeners.Select(x => x.Uri).Concat(KnownSubscribers.Select(x => x.Uri))
                .Distinct().ToArray();

            await lookups.ReadAliases(all);

            foreach (var subscriberAddress in KnownSubscribers)
            {
                subscriberAddress.ReadAlias(lookups);
            }

            var listeners = Listeners.ToArray();
            Listeners.Clear();

            foreach (var listener in listeners)
            {
                listener.ReadAlias(lookups);
                ListenForMessagesFrom(listener.Uri).MaximumParallelization(listener.MaximumParallelization);
            }
        }
    }
}
