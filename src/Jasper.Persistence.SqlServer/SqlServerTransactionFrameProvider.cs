using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Messaging;
using Jasper.Persistence.SqlServer.Persistence;

namespace Jasper.Persistence.SqlServer
{
    internal class SqlServerTransactionFrameProvider : ITransactionFrameProvider
    {
        public void ApplyTransactionSupport(IChain chain)
        {
            var shouldFlushOutgoingMessages = false;
            if (chain is RouteChain)
                shouldFlushOutgoingMessages = chain.As<RouteChain>().Action.Method.GetParameters()
                    .Any(x => x.ParameterType == typeof(IMessageContext));


            var frame = new SqlTransactionFrame {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
