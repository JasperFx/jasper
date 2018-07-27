using System.Linq;
using Jasper.Configuration;
using Jasper.Marten.Codegen;

namespace Jasper.Marten
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
