using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Persistence;

namespace IntegrationTests.Persistence.SqlServer.Persistence
{
    public class SendItemEndpoint
    {
        [Transactional]
        public Task post_send_item(ItemCreated created, IMessageContext context)
        {
            return context.Send(created);
        }
    }
}
