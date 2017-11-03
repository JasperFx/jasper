using System.Threading.Tasks;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public interface ISenderProtocol
    {
        Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch);
    }
}
