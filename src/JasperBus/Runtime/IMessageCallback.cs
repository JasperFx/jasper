using System;
using System.Threading.Tasks;

namespace JasperBus.Runtime
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed(Exception ex);

        void MoveToDelayedUntil(DateTime time);
        void MoveToErrors(ErrorReport report);
        Task Requeue(Envelope envelope);
        Task Send(Envelope envelope);

        bool SupportsSend { get; }
    }
}