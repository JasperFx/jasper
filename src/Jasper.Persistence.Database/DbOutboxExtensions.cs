using System.Data.Common;
using System.Threading.Tasks;

namespace Jasper.Persistence.Database
{
    public static class DbOutboxExtensions
    {
        public static Task EnlistInTransaction(this IExecutionContext context, DbTransaction tx)
        {
            var transaction = new DatabaseEnvelopeTransaction(context, tx);
            return context.EnlistInTransaction(transaction);
        }
    }
}
