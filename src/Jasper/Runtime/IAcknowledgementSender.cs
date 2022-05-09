using System.Threading.Tasks;

namespace Jasper.Runtime;

public interface IAcknowledgementSender
{
    Envelope BuildAcknowledgement(Envelope envelope);


    /// <summary>
    ///     Sends an acknowledgement back to the original sender
    /// </summary>
    /// <returns></returns>
    Task SendAcknowledgementAsync(Envelope envelope);

    /// <summary>
    ///     Send a failure acknowledgement back to the original
    ///     sending service
    /// </summary>
    /// <param name="original"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendFailureAcknowledgementAsync(Envelope original, string message);
}
