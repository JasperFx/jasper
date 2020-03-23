using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper
{
    // SAMPLE: IMissingHandler
    /// <summary>
    ///     Hook interface to receive notifications of envelopes received
    ///     that do not match any known handlers within the system
    /// </summary>
    public interface IMissingHandler
    {
        /// <summary>
        ///     Executes for unhandled envelopes
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        Task Handle(Envelope envelope, IMessagingRoot root);
    }

    // ENDSAMPLE
}
