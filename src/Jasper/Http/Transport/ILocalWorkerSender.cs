using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Http.Transport
{
    public interface ILocalWorkerSender
    {
        Task EnqueueDurably(params Envelope[] envelopes);
        Task EnqueueLightweight(params Envelope[] envelopes);
    }
}