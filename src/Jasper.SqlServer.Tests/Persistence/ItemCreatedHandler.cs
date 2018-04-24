using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class ItemCreatedHandler
    {
        [SqlTransaction]
        public static async Task Handle(ItemCreated created, SqlConnection conn, SqlTransaction tx, MessageTracker tracker, Envelope envelope)
        {
            await conn.CreateCommand(tx, "insert into receiver.item_created (id, name) values (@id, @name)")
                .With("id", created.Id)
                .With("name", created.Name)
                .ExecuteNonQueryAsync();

            tracker.Record(created, envelope);
        }
    }
}
