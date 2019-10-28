using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public class EFCoreEnvelopeTransaction : IEnvelopeTransaction
    {
        private readonly DbContext _db;
        private readonly IMessageContext _messaging;
        private DatabaseSettings _settings;
        private int _nodeId;

        public EFCoreEnvelopeTransaction(DbContext db, IMessageContext messaging)
        {
            if (messaging.Advanced.Persistence is DatabaseBackedEnvelopePersistence persistence)
            {
                _settings = persistence.Settings;
                _nodeId = persistence.Options.UniqueNodeId;
            }
            else
            {
                throw new InvalidOperationException(
                    "This Jasper application is not using Database backed message persistence. Please configure the message configuration");
            }

            _db = db;
            _messaging = messaging;
        }

        public Task Persist(Envelope envelope)
        {
            var outgoing = new OutgoingEnvelope(envelope);
            _db.Add(outgoing);

            return Task.CompletedTask;
        }

        public Task Persist(Envelope[] envelopes)
        {
            var outgoing = envelopes.Select(x => new OutgoingEnvelope(x));
            _db.AddRange(outgoing);

            return Task.CompletedTask;
        }

        public Task ScheduleJob(Envelope envelope)
        {
            var incoming = new IncomingEnvelope(envelope);
            _db.Add(incoming);

            return Task.CompletedTask;
        }

        public Task CopyTo(IEnvelopeTransaction other)
        {
            throw new NotSupportedException();
        }
    }
}
