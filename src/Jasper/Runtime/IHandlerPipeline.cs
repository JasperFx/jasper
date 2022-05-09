using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime;

public interface IHandlerPipeline
{
    Task InvokeAsync(Envelope envelope, IChannelCallback channel);
    Task InvokeAsync(Envelope envelope, IChannelCallback channel, Activity activity);
    Task InvokeNowAsync(Envelope envelope, CancellationToken cancellation = default);
}
