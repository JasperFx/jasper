using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Messaging;
using Jasper.Persistence.SqlServer.Persistence;

namespace Jasper.Persistence.SqlServer
{
    /// <summary>
    /// Marks this handler or http action method as using
    /// a Jasper-handled transaction lifecycle
    /// </summary>
    public class SqlTransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            var shouldFlushOutgoingMessages = false;
            if (chain is RouteChain)
            {
                shouldFlushOutgoingMessages = chain.As<RouteChain>().Action.Method.GetParameters()
                    .Any(x => x.ParameterType == typeof(IMessageContext));
            }


            var frame = new SqlTransactionFrame {ShouldFlushOutgoingMessages = shouldFlushOutgoingMessages};

            chain.Middleware.Add(frame);
        }
    }
}
