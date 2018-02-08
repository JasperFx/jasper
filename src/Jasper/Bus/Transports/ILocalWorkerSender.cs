using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports
{
    public interface ILocalWorkerSender
    {
        Task EnqueueDurably(params Envelope[] envelopes);
        Task EnqueueLightweight(params Envelope[] envelopes);
    }
}