using System.Linq;
using Jasper.Configuration;
using JasperBus.Marten.Codegen;

namespace JasperBus.Marten
{
    public class MartenTransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            if (!chain.Middleware.OfType<TransactionalFrame>().Any())
            {
                chain.Middleware.Add(new TransactionalFrame());
            }
        }
    }
}
