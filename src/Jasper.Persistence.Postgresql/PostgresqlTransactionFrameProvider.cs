using Jasper.Configuration;
using Jasper.Persistence.Database;
using Lamar;
using Npgsql;

namespace Jasper.Persistence.Postgresql;

internal class PostgresqlTransactionFrameProvider : ITransactionFrameProvider
{
    public void ApplyTransactionSupport(IChain chain, IContainer container)
    {
        var shouldFlushOutgoingMessages = chain.ShouldFlushOutgoingMessages();


        var frame = new DbTransactionFrame<NpgsqlTransaction, NpgsqlConnection>
            { ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages };

        chain.Middleware.Add(frame);
    }
}
