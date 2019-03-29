using Jasper.Persistence;
using LamarCodeGeneration;
using LamarCompiler;

namespace Jasper.Configuration
{
    public class JasperGenerationRules : GenerationRules
    {
        public JasperGenerationRules(string applicationNamespace) : base(applicationNamespace)
        {
        }

        /// <summary>
        ///     The currently known strategy for persisting saga state
        /// </summary>
        public ISagaPersistenceFrameProvider SagaPersistence { get; set; } = new InMemorySagaPersistenceFrameProvider();

        /// <summary>
        ///     The currently known strategy for code generating transaction middleware
        /// </summary>
        public ITransactionFrameProvider Transactions { get; set; } = new NulloTransactionFrameProvider();

        public static JasperGenerationRules Empty()
        {
            return new JasperGenerationRules("Empty");
        }

        public class NulloTransactionFrameProvider : ITransactionFrameProvider
        {
            public void ApplyTransactionSupport(IChain chain)
            {
                // Nothing
            }
        }
    }
}
