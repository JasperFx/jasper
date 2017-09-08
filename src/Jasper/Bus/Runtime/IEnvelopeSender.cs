using System.Threading.Tasks;
using Jasper.Bus.Transports;

namespace Jasper.Bus.Runtime
{
    // Tested strictly through integration tests
    public interface IEnvelopeSender
    {
        Task<string> Send(Envelope envelope);
        Task<string> Send(Envelope envelope, IMessageCallback callback);
        Task EnqueueLocally(object message);
    }
}
