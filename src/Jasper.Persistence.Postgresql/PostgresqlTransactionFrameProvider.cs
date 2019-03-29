using System.Linq;
using Baseline;
using Jasper.Http.Model;
using Jasper.Messaging;
using Jasper.Persistence.Database;
using Npgsql;

namespace Jasper.Persistence.Postgresql
{
    internal class PostgresqlTransactionFrameProvider : ITransactionFrameProvider
    {
        public void ApplyTransactionSupport(IChain chain)
        {
            var shouldFlushOutgoingMessages = false;
            if (chain is RouteChain)
                shouldFlushOutgoingMessages = chain.As<RouteChain>().Action.Method.GetParameters()
                    .Any(x => x.ParameterType == typeof(IMessageContext));


            var frame = new DbTransactionFrame<NpgsqlTransaction, NpgsqlConnection> {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
