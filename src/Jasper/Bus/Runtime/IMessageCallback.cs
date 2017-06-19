using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;

namespace Jasper.Bus.Runtime
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed(Exception ex);

        Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time);
        void MoveToErrors(ErrorReport report);
        Task Requeue(Envelope envelope);
        Task Send(Envelope envelope);

        bool SupportsSend { get; }
    }
}