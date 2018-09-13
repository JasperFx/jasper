using Jasper.Configuration;

namespace Jasper.Persistence
{
    /// <summary>
    /// Applies unit of work / transactional boundary middleware to the
    /// current chain using the currently configured persistence
    /// </summary>
    public class TransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain, JasperGenerationRules rules)
        {
            rules.Transactions.ApplyTransactionSupport(chain);
        }
    }
}
