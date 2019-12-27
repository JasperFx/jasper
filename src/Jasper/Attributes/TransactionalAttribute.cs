using Jasper.Configuration;
using Jasper.Persistence.Sagas;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;

namespace Jasper.Attributes
{
    /// <summary>
    ///     Applies unit of work / transactional boundary middleware to the
    ///     current chain using the currently configured persistence
    /// </summary>
    public class TransactionalAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain, GenerationRules rules, IContainer container)
        {
            rules.As<GenerationRules>().GetTransactions().ApplyTransactionSupport(chain, container);
        }
    }
}
