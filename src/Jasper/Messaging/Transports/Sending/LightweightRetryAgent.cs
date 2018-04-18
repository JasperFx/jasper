using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public class LightweightRetryAgent : RetryAgent
    {
        public LightweightRetryAgent(ISender sender, RetrySettings settings) : base(sender, settings)
        {
        }

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
            {
                // It's perfectly okay to not wait on the task here
                _sender.Enqueue(envelope);
            }

            return Task.CompletedTask;
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();
    }
}
