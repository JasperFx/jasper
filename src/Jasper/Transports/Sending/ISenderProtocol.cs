using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public interface ISenderProtocol
    {
        Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch);
    }
}
