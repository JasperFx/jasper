using Jasper.Configuration;
using Jasper.Messaging.Model;
using LamarCodeGeneration;

namespace Jasper.Messaging.Configuration
{
    // SAMPLE: IHandlerPolicy
    /// <summary>
    ///     Use to apply your own conventions or policies to message handlers
    /// </summary>
    public interface IHandlerPolicy
    {
        /// <summary>
        ///     Called during bootstrapping to alter how the message handlers are configured
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="rules"></param>
        void Apply(HandlerGraph graph, GenerationRules rules);
    }
    // ENDSAMPLE
}
