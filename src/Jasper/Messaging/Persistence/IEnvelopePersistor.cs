using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Persistence
{
    public interface IEnvelopePersistor
    {
        Task DeleteIncomingEnvelopes(Envelope[] envelopes);
        Task DeleteOutgoingEnvelopes(Envelope[] envelopes);
        Task MoveToDeadLetterStorage(ErrorReport[] errors);
        Task ScheduleExecution(Envelope[] envelopes);

        Task<ErrorReport> LoadDeadLetterEnvelope(Guid id);
    }
}
