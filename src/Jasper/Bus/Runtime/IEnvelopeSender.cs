using System.Threading.Tasks;

namespace Jasper.Bus.Runtime
{
    // Tested strictly through integration tests
    public interface IEnvelopeSender
    {
        Task<string> Send(Envelope envelope);
        Task<string> Send(Envelope envelope, IMessageCallback callback);
    }
}