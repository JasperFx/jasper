using System;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusTransport : TransportBase
    {
        private readonly AzureServiceBusSettings _asbSettings;
        private readonly IEnvelopeMapper _mapper;

        public AzureServiceBusTransport(IDurableMessagingFactory factory, ITransportLogger logger, JasperOptions options, AzureServiceBusSettings asbSettings, IEnvelopeMapper mapper)
            : base("azureservicebus", factory, logger, options)
        {
            _asbSettings = asbSettings;
            _mapper = mapper;
        }

        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            return new AzureServiceBusSender(uri, _mapper, _asbSettings, logger, cancellation);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            return incoming;
        }

        protected override IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings)
        {
            return new AzureServiceBusListeningAgent(_asbSettings, _mapper, uri, logger);
        }


    }
}
