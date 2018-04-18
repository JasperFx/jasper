using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Persistence
{
    public interface IEnvelopePersistor
    {
        Task Persist(Envelope envelope);
        Task Persist(IEnumerable<Envelope> envelopes);
        Task ScheduleJob(Envelope envelope);

        Task CopyTo(IEnvelopePersistor other);
    }
}
