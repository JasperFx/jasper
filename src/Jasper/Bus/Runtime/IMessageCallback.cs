using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed(Exception ex);

        Task MoveToDelayedUntil(DateTime time);
        void MoveToErrors(ErrorReport report);
        Task Requeue(Envelope envelope);
        Task Send(Envelope envelope);

        bool SupportsSend { get; }
    }
}