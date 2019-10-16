using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Persistence.Database;

namespace Jasper.Persistence.SqlServer
{
    internal class SqlServerTransactionFrameProvider : ITransactionFrameProvider
    {
        public void ApplyTransactionSupport(IChain chain)
        {
            var shouldFlushOutgoingMessages = chain.ShouldFlushOutgoingMessages();


            var frame = new DbTransactionFrame<SqlTransaction, SqlConnection>
                {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
