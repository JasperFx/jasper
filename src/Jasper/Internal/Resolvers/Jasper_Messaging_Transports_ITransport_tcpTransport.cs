using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_Messaging_Transports_ITransport_tcpTransport
    public class Jasper_Messaging_Transports_ITransport_tcpTransport : Lamar.IoC.Resolvers.TransientResolver<Jasper.Messaging.Transports.ITransport>
    {
        private readonly Jasper.Messaging.Logging.ITransportLogger _transportLogger_307335187;
        private readonly Jasper.Messaging.Transports.Configuration.MessagingSettings _messagingSettings;

        public Jasper_Messaging_Transports_ITransport_tcpTransport([Lamar.Named("transportLogger2")] Jasper.Messaging.Logging.ITransportLogger transportLogger_307335187, Jasper.Messaging.Transports.Configuration.MessagingSettings messagingSettings)
        {
            _transportLogger_307335187 = transportLogger_307335187;
            _messagingSettings = messagingSettings;
        }



        public override Jasper.Messaging.Transports.ITransport Build(Lamar.IoC.Scope scope)
        {
            var nulloDurableMessagingFactory = new Jasper.Messaging.Transports.NulloDurableMessagingFactory(_transportLogger_307335187, _messagingSettings);
            return new Jasper.Messaging.Transports.Tcp.TcpTransport(nulloDurableMessagingFactory, _transportLogger_307335187, _messagingSettings);
        }

    }

    // END: Jasper_Messaging_Transports_ITransport_tcpTransport
    
    
}

