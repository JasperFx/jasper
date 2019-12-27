using System.Threading.Tasks;
using Jasper.Transports.Tcp;

namespace Jasper.Transports.Sending
{
    public interface ISenderProtocol
    {
        Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch);
    }
}
