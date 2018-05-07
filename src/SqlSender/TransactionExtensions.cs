using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.SqlServer.Util;

namespace SqlSender
{
    public static class TransactionExtensions
    {
        public static Task StoreSent(this SqlTransaction tx, Guid id, string messageType)
        {
            return tx.Connection.CreateCommand("insert into sender.sent_track (id, message_type) values (@id, @type)")
                .With("id", id)
                .With("type", messageType)
                .ExecuteNonQueryAsync();
        }

        public static Task StoreReceived(this SqlTransaction tx, Guid id, string messageType)
        {
            return tx.Connection.CreateCommand("insert into sender.received_track (id, message_type) values (@id, @type)")
                .With("id", id)
                .With("type", messageType)
                .ExecuteNonQueryAsync();
        }
    }
}
