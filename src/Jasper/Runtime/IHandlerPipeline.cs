using System.Diagnostics;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public interface IHandlerPipeline
    {

        Task Invoke(Envelope envelope, IChannelCallback channel);
        Task Invoke(Envelope envelope, IChannelCallback channel, Activity activity);
        Task InvokeNow(Envelope envelope);
    }
}
