using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;

namespace Jasper.Bus.Runtime
{
    public interface IMessageCallback
    {
        Task MarkSuccessful();
        Task MarkFailed(Exception ex);

        Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time);
        Task MoveToErrors(ErrorReport report);
        Task Requeue(Envelope envelope);
        Task Send(Envelope envelope);

        bool SupportsSend { get; }
    }
}
