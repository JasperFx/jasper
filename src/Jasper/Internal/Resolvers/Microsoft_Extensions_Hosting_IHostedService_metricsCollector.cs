using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Microsoft_Extensions_Hosting_IHostedService_metricsCollector
    public class Microsoft_Extensions_Hosting_IHostedService_metricsCollector : Lamar.IoC.Resolvers.TransientResolver<Microsoft.Extensions.Hosting.IHostedService>
    {
        private readonly Jasper.Messaging.Logging.IMessageLogger _messageLogger;
        private readonly Jasper.Messaging.Transports.Configuration.MessagingSettings _messagingSettings;
        private readonly Jasper.Messaging.IMessagingRoot _messagingRoot;

        public Microsoft_Extensions_Hosting_IHostedService_metricsCollector(Jasper.Messaging.Logging.IMessageLogger messageLogger, Jasper.Messaging.Transports.Configuration.MessagingSettings messagingSettings, Jasper.Messaging.IMessagingRoot messagingRoot)
        {
            _messageLogger = messageLogger;
            _messagingSettings = messagingSettings;
            _messagingRoot = messagingRoot;
        }



        public override Microsoft.Extensions.Hosting.IHostedService Build(Lamar.IoC.Scope scope)
        {
            var workerQueue = _messagingRoot.Workers;
            var nulloEnvelopePersistor = new Jasper.Messaging.Durability.NulloEnvelopePersistor();
            var nulloMetrics = new Jasper.Messaging.Logging.NulloMetrics();
            return new Jasper.Messaging.Logging.MetricsCollector(nulloMetrics, nulloEnvelopePersistor, _messageLogger, _messagingSettings, workerQueue);
        }

    }

    // END: Microsoft_Extensions_Hosting_IHostedService_metricsCollector
    
    
}

