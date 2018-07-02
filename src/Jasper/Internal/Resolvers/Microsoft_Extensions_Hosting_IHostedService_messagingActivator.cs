using Jasper;
using Jasper.Messaging;
using Lamar.IoC;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Microsoft_Extensions_Hosting_IHostedService_messagingActivator
    public class Microsoft_Extensions_Hosting_IHostedService_messagingActivator : Lamar.IoC.Resolvers.SingletonResolver<Microsoft.Extensions.Hosting.IHostedService>
    {
        private readonly Jasper.JasperRuntime _jasperRuntime;
        private readonly Jasper.Messaging.IMessagingRoot _messagingRoot;
        private readonly Lamar.IoC.Scope _topLevelScope;

        public Microsoft_Extensions_Hosting_IHostedService_messagingActivator(Jasper.JasperRuntime jasperRuntime, Jasper.Messaging.IMessagingRoot messagingRoot, Lamar.IoC.Scope topLevelScope) : base(topLevelScope)
        {
            _jasperRuntime = jasperRuntime;
            _messagingRoot = messagingRoot;
            _topLevelScope = topLevelScope;
        }



        public override Microsoft.Extensions.Hosting.IHostedService Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.Messaging.MessagingActivator(_jasperRuntime, _messagingRoot);
        }

    }

    // END: Microsoft_Extensions_Hosting_IHostedService_messagingActivator
    
    
}

