using System.Data.Common;
using System.Threading.Tasks;

namespace Jasper.Persistence.Database;

public static class DbOutboxExtensions
{
    public static Task EnlistInTransactionAsync(this IExecutionContext context, DbTransaction tx)
    {
        var transaction = new DatabaseEnvelopeTransaction(context, tx);
        return context.EnlistInTransactionAsync(transaction);
    }
}
