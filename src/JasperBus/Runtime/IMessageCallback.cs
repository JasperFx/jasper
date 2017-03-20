using System;

namespace JasperBus.Runtime
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed(Exception ex);

        void MoveToDelayedUntil(DateTime time);
        void MoveToErrors(ErrorReport report);
        void Requeue(Envelope envelope);
        void Send(Envelope envelope);

        bool SupportsSend { get; }
    }
}