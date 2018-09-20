using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: ResolverLoader
    public class ResolverLoader : Lamar.IoC.Exports.IResolverLoader
    {


        public System.Collections.Generic.Dictionary<string, System.Type> ResolverTypes()
        {
            return new System.Collections.Generic.Dictionary<string, System.Type>{{"Microsoft_Extensions_Hosting_IHostedService_messagingActivator", typeof(Jasper.Internal.Resolvers.Microsoft_Extensions_Hosting_IHostedService_messagingActivator)}, {"Microsoft_Extensions_Hosting_IHostedService_metricsCollector", typeof(Jasper.Internal.Resolvers.Microsoft_Extensions_Hosting_IHostedService_metricsCollector)}, {"Microsoft_Extensions_Hosting_IHostedService_backPressureAgent", typeof(Jasper.Internal.Resolvers.Microsoft_Extensions_Hosting_IHostedService_backPressureAgent)}, {"Jasper_Messaging_Logging_IMessageLogger_messageLogger", typeof(Jasper.Internal.Resolvers.Jasper_Messaging_Logging_IMessageLogger_messageLogger)}, {"Jasper_Messaging_Logging_ITransportLogger_transportLogger1", typeof(Jasper.Internal.Resolvers.Jasper_Messaging_Logging_ITransportLogger_transportLogger1)}, {"Jasper_Messaging_Logging_ITransportLogger_transportLogger2", typeof(Jasper.Internal.Resolvers.Jasper_Messaging_Logging_ITransportLogger_transportLogger2)}, {"Jasper_Messaging_Transports_ITransport_tcpTransport", typeof(Jasper.Internal.Resolvers.Jasper_Messaging_Transports_ITransport_tcpTransport)}, {"Jasper_Messaging_IMessagingRoot_messagingRoot", typeof(Jasper.Internal.Resolvers.Jasper_Messaging_IMessagingRoot_messagingRoot)}, {"Jasper_EnvironmentChecks_IEnvironmentRecorder_environmentRecorder", typeof(Jasper.Internal.Resolvers.Jasper_EnvironmentChecks_IEnvironmentRecorder_environmentRecorder)}, {"Microsoft_AspNetCore_Hosting_Server_IServer_nulloServer", typeof(Jasper.Internal.Resolvers.Microsoft_AspNetCore_Hosting_Server_IServer_nulloServer)}};
        }

    }

    // END: ResolverLoader
    
    
}

