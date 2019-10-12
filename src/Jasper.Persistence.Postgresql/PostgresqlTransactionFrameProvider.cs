using Jasper.Configuration;
using Jasper.Persistence.Database;
using Npgsql;

namespace Jasper.Persistence.Postgresql
{
    internal class PostgresqlTransactionFrameProvider : ITransactionFrameProvider
    {
        public void ApplyTransactionSupport(IChain chain)
        {
            var shouldFlushOutgoingMessages = chain.ShouldFlushOutgoingMessages();


            var frame = new DbTransactionFrame<NpgsqlTransaction, NpgsqlConnection>
                {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
