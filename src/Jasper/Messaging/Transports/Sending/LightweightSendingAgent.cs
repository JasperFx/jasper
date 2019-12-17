using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public class LightweightSendingAgent : SendingAgent
    {
        public LightweightSendingAgent(ITransportLogger logger, IMessageLogger messageLogger, ISender sender, AdvancedSettings settings) : base(logger, messageLogger, sender, settings)
        {
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();

        public override Task EnqueueForRetry(OutgoingMessageBatch batch)
        {
            Queued.AddRange(batch.Messages);
            Queued.RemoveAll(e => e.IsExpired());

            if (Queued.Count > _settings.MaximumEnvelopeRetryStorage)
            {
                var toRemove = Queued.Count - _settings.MaximumEnvelopeRetryStorage;
                Queued = Queued.Skip(toRemove).ToList();
            }

            return Task.CompletedTask;
        }

        protected override Task afterRestarting(ISender sender)
        {
            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued.Clear();

            foreach (var envelope in toRetry)
                // It's perfectly okay to not wait on the task here
                _sender.Enqueue(envelope);

            return Task.CompletedTask;
        }

        public override Task Successful(OutgoingMessageBatch outgoing)
        {
            return MarkSuccess();
        }

        public override Task Successful(Envelope outgoing)
        {
            return MarkSuccess();
        }

        protected override Task storeAndForward(Envelope envelope)
        {
            return _sender.Enqueue(envelope);
        }

        public override bool IsDurable { get; } = false;
    }
}
