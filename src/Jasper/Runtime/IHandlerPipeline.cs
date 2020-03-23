using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public interface IHandlerPipeline
    {

        Task Invoke(Envelope envelope, IChannelCallback channel);
        Task InvokeNow(Envelope envelope);
    }
}
