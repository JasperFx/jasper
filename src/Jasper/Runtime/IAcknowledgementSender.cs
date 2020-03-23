using System.Threading.Tasks;

namespace Jasper.Runtime
{
    public interface IAcknowledgementSender
    {
        Envelope BuildAcknowledgement(Envelope envelope);


        /// <summary>
        ///     Sends an acknowledgement back to the original sender
        /// </summary>
        /// <returns></returns>
        Task SendAcknowledgement(Envelope envelope);

        /// <summary>
        ///     Send a failure acknowledgement back to the original
        ///     sending service
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        Task SendFailureAcknowledgement(Envelope original, string message);

    }
}