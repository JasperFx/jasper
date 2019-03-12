using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports
{
    public interface ILoopbackWorkerSender
    {
        Task EnqueueDurably(params Envelope[] envelopes);
        Task EnqueueLightweight(params Envelope[] envelopes);
    }
}
