using System.Data.Common;
using System.Threading.Tasks;

namespace Jasper.Persistence.Database;

public static class DbOutboxExtensions
{
    public static Task EnlistInOutboxAsync(this IExecutionContext context, DbTransaction tx)
    {
        var transaction = new DatabaseEnvelopeOutbox(context, tx);
        return context.EnlistInOutboxAsync(transaction);
    }
}
