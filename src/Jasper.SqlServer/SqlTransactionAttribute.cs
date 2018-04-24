using System;
using Jasper.Configuration;
using Jasper.SqlServer.Persistence;

namespace Jasper.SqlServer
{
    /// <summary>
    /// Marks this handler or http action method as using
    /// a Jasper-handled transaction lifecycle
    /// </summary>
    public class SqlTransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            chain.Middleware.Add(new SqlTransactionFrame());
        }
    }
}
