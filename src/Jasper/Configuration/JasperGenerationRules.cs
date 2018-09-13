using Jasper.Messaging.Sagas;
using Jasper.Persistence;
using Lamar.Codegen;

namespace Jasper.Configuration
{
    public class JasperGenerationRules : GenerationRules
    {
        public static JasperGenerationRules Empty()
        {
            return new JasperGenerationRules("Empty");
        }

        /// <summary>
        /// The currently known strategy for persisting saga state
        /// </summary>
        public ISagaPersistenceFrameProvider SagaPersistence { get; set; } = new InMemorySagaPersistenceFrameProvider();

        /// <summary>
        /// The currently known strategy for code generating transaction middleware
        /// </summary>
        public ITransactionFrameProvider Transactions { get; set; } = new NulloTransactionFrameProvider();

        public JasperGenerationRules(string applicationNamespace) : base(applicationNamespace)
        {
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
