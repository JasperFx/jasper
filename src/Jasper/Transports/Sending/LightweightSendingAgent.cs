using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;

namespace Jasper.Transports.Sending
{
    public class LightweightSendingAgent : SendingAgent
    {
        public LightweightSendingAgent(ITransportLogger logger, IMessageLogger messageLogger, ISender sender,
            AdvancedSettings settings, Endpoint endpoint) : base(logger, messageLogger, sender, settings, endpoint)
        {
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();

        public override Task EnqueueForRetry(OutgoingMessageBatch batch)
        {
            Queued.AddRange(batch.Messages);
            Queued.RemoveAll(e => e.IsExpired());

            if (Queued.Count > Endpoint.MaximumEnvelopeRetryStorage)
            {
                var toRemove = Queued.Count - Endpoint.MaximumEnvelopeRetryStorage;
                Queued = Queued.Skip(toRemove).ToList();
            }

            return Task.CompletedTask;
        }

        protected override Task afterRestarting(ISender sender)
        {
            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued.Clear();

            foreach (var envelope in toRetry)
            {
                // It's perfectly okay to not wait on the task here
                _senderDelegate(envelope);
            }

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
            return _senderDelegate(envelope);
        }

        public override bool IsDurable { get; } = false;
    }
}
