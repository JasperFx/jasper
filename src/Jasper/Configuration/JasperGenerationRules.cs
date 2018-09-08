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
        public IPersistence Persistence { get; set; } = new InMemoryPersistence();

        public JasperGenerationRules(string applicationNamespace) : base(applicationNamespace)
        {
        }
    }
}
