using System.Threading.Tasks;

namespace Jasper.Transports.Sending;

public interface ISenderProtocol
{
    Task SendBatchAsync(ISenderCallback callback, OutgoingMessageBatch batch);
}
