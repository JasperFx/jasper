using System.Threading.Tasks;
using Jasper.Attributes;
using Microsoft.Data.SqlClient;
using Weasel.Core;

namespace Jasper.Persistence.Testing.SqlServer.Durability
{
    public class ItemCreatedHandler
    {
        [Transactional]
        public static async Task Handle(
            ItemCreated created,
            SqlTransaction tx // the current transaction
        )
        {
            // Using some extension method helpers inside of Jasper here
            await tx.CreateCommand("insert into receiver.item_created (id, name) values (@id, @name)")
                .With("id", created.Id)
                .With("name", created.Name)
                .ExecuteNonQueryAsync();
        }
    }
}