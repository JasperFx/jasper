using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    /// <summary>
    /// Use to apply your own conventions or policies to message handlers
    /// </summary>
    public interface IHandlerPolicy
    {
        /// <summary>
        /// Called during bootstrapping to alter how the message handlers are configured
        /// </summary>
        /// <param name="graph"></param>
        void Apply(HandlerGraph graph);
    }
}
