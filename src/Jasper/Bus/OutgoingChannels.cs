using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus
{
    public class OutgoingChannels : IChannelGraph
    {
        private readonly IDictionary<Uri, IChannel> _channels = new ConcurrentDictionary<Uri, IChannel>();

        public void Add(Uri uri, IChannel channel)
        {
            _channels[uri] = channel;
        }

        public IEnumerator<IChannel> GetEnumerator()
        {
            return _channels.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IChannel this[Uri uri] => _channels[uri];

        public IChannel DefaultChannel { get; private set; }
        public IChannel DefaultRetryChannel => this[TransportConstants.RetryUri];
        public string Name { get; private set; }
        public string[] ValidTransports { get; private set; }

        public IChannel TryGetChannel(Uri address)
        {
            return _channels.ContainsKey(address) ? _channels[address] : null;
        }

        public bool HasChannel(Uri uri)
        {
            return _channels.ContainsKey(uri);
        }

        public void StartTransports(IHandlerPipeline pipeline, BusSettings settings, ITransport[] transports)
        {
            Name = settings.ServiceName;
            ValidTransports = transports.Select(x => x.Protocol).ToArray();


            foreach (var transport in transports)
            {
                var channels = transport.Start(pipeline, settings, this);
                foreach (var channel in channels)
                {
                    _channels[channel.Uri] = channel;
                }
            }

            if (settings.DefaultChannelAddress != null)
            {
                DefaultChannel = this[settings.DefaultChannelAddress];
            }

            assertNoUnknownTransportsInSubscribers(settings);


            assertNoUnknownTransportsInListeners(settings);
        }

        private void assertNoUnknownTransportsInListeners(BusSettings settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();

            if (unknowns.Any())
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
            }
        }

        private void assertNoUnknownTransportsInSubscribers(BusSettings settings)
        {
            var unknowns = settings.KnownSubscribers.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
            }
        }
    }
}
