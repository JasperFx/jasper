using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Persistence;
using Jasper.Messaging.Runtime;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenEnvelopePersistor : IEnvelopePersistor
    {
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _tables;

        public MartenEnvelopePersistor(IDocumentStore store, EnvelopeTables tables)
        {
            _store = store;
            _tables = tables;
        }

        public async Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                session.DeleteEnvelopes(_tables.Incoming, envelopes);
                await session.SaveChangesAsync();
            }
        }

        public async Task DeleteOutgoingEnvelopes(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                session.DeleteEnvelopes(_tables.Outgoing, envelopes);
                await session.SaveChangesAsync();
            }
        }

        public async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            using (var session = _store.LightweightSession())
            {
                session.Store(errors);
                session.DeleteEnvelopes(_tables.Incoming, errors.Select(x => x.Id).ToArray());
                await session.SaveChangesAsync();
            }
        }

        public async Task ScheduleExecution(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                foreach (var envelope in envelopes)
                {
                    session.ScheduleExecution(_tables.Incoming, envelope);
                }

                await session.SaveChangesAsync();
            }
        }


    }
}
