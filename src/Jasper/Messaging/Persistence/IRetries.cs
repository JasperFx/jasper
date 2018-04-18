using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Persistence
{
    public interface IRetries
    {
        void DeleteIncoming(Envelope envelope);
        void DeleteOutgoing(Envelope envelope);
        void LogErrorReport(ErrorReport report);
        void Dispose();
        void ScheduleExecution(Envelope envelope);
    }
}
