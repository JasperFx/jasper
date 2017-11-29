using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
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

        protected override void afterRestarting()
        {
            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued.Clear();

            foreach (var envelope in toRetry)
            {
                // It's perfectly okay to not wait on the task here
                _sender.Enqueue(envelope);
            }
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();
    }
}
