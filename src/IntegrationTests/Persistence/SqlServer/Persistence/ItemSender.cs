using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.SqlServer;

namespace IntegrationTests.Persistence.SqlServer.Persistence
{


    public class SendItemEndpoint
    {
        [SqlTransaction]
        public Task post_send_item(ItemCreated created, IMessageContext context)
        {
            return context.Send(created);
        }
    }
}
