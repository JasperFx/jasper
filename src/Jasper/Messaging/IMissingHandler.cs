using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging
{
    // SAMPLE: IMissingHandler
    /// <summary>
    /// Hook interface to receive notifications of envelopes received
    /// that do not match any known handlers within the system
    /// </summary>
    public interface IMissingHandler
    {
        /// <summary>
        /// Executes for unhandled envelopes
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Handle(Envelope envelope, IMessageContext context);
    }
    // ENDSAMPLE
}
