using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime
{
    // Tested strictly through integration tests
    public interface IEnvelopeSender
    {
        Task<string> Send(Envelope envelope);
        Task<string> Send(Envelope envelope, IMessageCallback callback);
        Task EnqueueLocally(object message);
    }

    public class NoRoutesException : Exception
    {
        public NoRoutesException(Envelope envelope) : base($"Could not determine any valid routes for {envelope}")
        {
        }
    }
}
