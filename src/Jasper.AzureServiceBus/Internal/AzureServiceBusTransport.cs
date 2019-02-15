using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusTransport : ExternalTransportBase<AzureServiceBusSettings, AzureServiceBusEndpoint>
    {
        public const string ProtocolName = "azureservicebus";


        public AzureServiceBusTransport(AzureServiceBusSettings settings, IDurableMessagingFactory factory, ITransportLogger logger, JasperOptions jasperOptions)
            : base(ProtocolName, settings, factory, logger, jasperOptions)
        {
        }

        protected override ISender buildSender(TransportUri transportUri, AzureServiceBusEndpoint endpoint, CancellationToken cancellation)
        {
            return new AzureServiceBusSender(endpoint, logger, cancellation);
        }

        protected override IListeningAgent buildListeningAgent(TransportUri transportUri,
            AzureServiceBusEndpoint endpoint,
            JasperOptions settings, HandlerGraph handlers)
        {
            return new AzureServiceBusListeningAgent(endpoint, handlers, logger, settings.Cancellation);
        }
    }
}
