using Jasper;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Microsoft_Extensions_Hosting_IHostedService_nodeRegistration
    public class Microsoft_Extensions_Hosting_IHostedService_nodeRegistration : Lamar.IoC.Resolvers.TransientResolver<Microsoft.Extensions.Hosting.IHostedService>
    {
        private readonly Jasper.Messaging.Transports.Configuration.MessagingSettings _messagingSettings;
        private readonly Jasper.Messaging.Runtime.Subscriptions.INodeDiscovery _nodeDiscovery;
        private readonly Jasper.JasperRuntime _jasperRuntime;
        private readonly Jasper.Messaging.Logging.IMessageLogger _messageLogger;

        public Microsoft_Extensions_Hosting_IHostedService_nodeRegistration(Jasper.Messaging.Transports.Configuration.MessagingSettings messagingSettings, Jasper.Messaging.Runtime.Subscriptions.INodeDiscovery nodeDiscovery, Jasper.JasperRuntime jasperRuntime, Jasper.Messaging.Logging.IMessageLogger messageLogger)
        {
            _messagingSettings = messagingSettings;
            _nodeDiscovery = nodeDiscovery;
            _jasperRuntime = jasperRuntime;
            _messageLogger = messageLogger;
        }



        public override Microsoft.Extensions.Hosting.IHostedService Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.Messaging.NodeRegistration(_messagingSettings, _nodeDiscovery, _jasperRuntime, _messageLogger);
        }

    }

    // END: Microsoft_Extensions_Hosting_IHostedService_nodeRegistration
    
    
}

