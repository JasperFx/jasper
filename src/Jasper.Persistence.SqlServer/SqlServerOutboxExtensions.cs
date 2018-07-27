using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Persistence.SqlServer.Persistence;

namespace Jasper.Persistence.SqlServer
{
    public static class SqlServerOutboxExtensions
    {
        public static Task EnlistInTransaction(this IMessageContext context, SqlTransaction tx)
        {
            var transaction = new SqlServerEnvelopeTransaction(context, tx);
            return context.EnlistInTransaction(transaction);
        }


    }
}
