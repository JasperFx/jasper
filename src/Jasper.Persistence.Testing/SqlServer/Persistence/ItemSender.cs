using System.Threading.Tasks;
using Jasper.Messaging;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
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
