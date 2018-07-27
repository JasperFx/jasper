using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Servers;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class ItemSender : JasperRegistry
    {
        public ItemSender()
        {
            Settings.PersistMessagesWithSqlServer(SqlServerContainer.ConnectionString, "sender");



            Publish.Message<ItemCreated>().To("tcp://localhost:2345/durable");
            Publish.Message<Question>().To("tcp://localhost:2345/durable");

            Transports.LightweightListenerAt(2567);



        }
    }

    public class SendItemEndpoint
    {
        [SqlTransaction]
        public Task post_send_item(ItemCreated created, IMessageContext context)
        {
            return context.Send(created);
        }
    }
}
