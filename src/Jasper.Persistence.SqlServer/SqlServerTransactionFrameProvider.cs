using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Lamar;

namespace Jasper.Persistence.SqlServer
{
    internal class SqlServerTransactionFrameProvider : ITransactionFrameProvider
    {
        public void ApplyTransactionSupport(IChain chain, IContainer container)
        {
            var shouldFlushOutgoingMessages = chain.ShouldFlushOutgoingMessages();


            var frame = new DbTransactionFrame<SqlTransaction, SqlConnection>
                {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
