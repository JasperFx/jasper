using System.Linq;
using Jasper.Configuration;
using Jasper.Persistence.Marten.Codegen;

namespace Jasper.Persistence.Marten
{
    /// <summary>
    /// Applies Marten unit of work / transactional boundary middleware to the
    /// current chain
    /// </summary>
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
