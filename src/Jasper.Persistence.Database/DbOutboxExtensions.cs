using System.Data.Common;
using System.Threading.Tasks;
using Jasper.Messaging;

namespace Jasper.Persistence.Database
{
    public static class DbOutboxExtensions
    {
        public static Task EnlistInTransaction(this IMessageContext context, DbTransaction tx)
        {
            var transaction = new DatabaseEnvelopeTransaction(context, tx);
            return context.EnlistInTransaction(transaction);
        }
    }
}
