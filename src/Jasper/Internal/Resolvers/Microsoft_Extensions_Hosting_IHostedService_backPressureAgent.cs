using Jasper.Messaging;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Microsoft_Extensions_Hosting_IHostedService_backPressureAgent
    public class Microsoft_Extensions_Hosting_IHostedService_backPressureAgent : Lamar.IoC.Resolvers.TransientResolver<Microsoft.Extensions.Hosting.IHostedService>
    {
        private readonly Jasper.Messaging.IMessagingRoot _messagingRoot;

        public Microsoft_Extensions_Hosting_IHostedService_backPressureAgent(Jasper.Messaging.IMessagingRoot messagingRoot)
        {
            _messagingRoot = messagingRoot;
        }



        public override Microsoft.Extensions.Hosting.IHostedService Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.Messaging.BackPressureAgent(_messagingRoot);
        }

    }

    // END: Microsoft_Extensions_Hosting_IHostedService_backPressureAgent
    
    
}

