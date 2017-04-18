using System.Collections.Generic;
using System.Threading.Tasks;

namespace JasperBus.Runtime
{
    // Tested strictly through integration tests
    public interface IEnvelopeSender
    {
        Task<string> Send(Envelope envelope);
        Task<string> Send(Envelope envelope, IMessageCallback callback);
    }
}