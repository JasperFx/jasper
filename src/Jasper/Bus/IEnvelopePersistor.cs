using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public interface IEnvelopePersistor
    {
        Task Persist(Envelope envelope);
        Task Persist(IEnumerable<Envelope> envelopes);
        Task ScheduleJob(Envelope envelope);
    }
}
