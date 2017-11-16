using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public class LightweightRetryAgent : IDisposable
    {
        private readonly ISender _sender;

        private int _failureCount = 0;
        private Pinger _pinger;
        private readonly RetrySettings _settings;

        public LightweightRetryAgent(ISender sender, RetrySettings settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public void MarkFailed(OutgoingMessageBatch batch)
        {
            _failureCount++;

            if (_failureCount >= _settings.FailuresBeforeCircuitBreaks)
            {
                _sender.Latch();
                EnqueueForRetry(batch);
                _pinger = new Pinger(_sender, _settings.Cooldown, restartSending);


            }
            else
            {
                foreach (var envelope in batch.Messages)
                {
                    // Not worried about the await here
                    _sender.Enqueue(envelope);
                }
            }


        }

        public void EnqueueForRetry(OutgoingMessageBatch batch)
        {
            Queued.AddRange(batch.Messages);
            Queued.RemoveAll(e => e.IsExpired());

            if (Queued.Count > _settings.MaximumEnvelopeRetryStorage)
            {
                var toRemove = Queued.Count - _settings.MaximumEnvelopeRetryStorage;
                Queued = Queued.Skip(toRemove).ToList();
            }
        }

        private void restartSending()
        {
            _pinger.Dispose();
            _pinger = null;

            _sender.Unlatch();

            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued.Clear();

            foreach (var envelope in toRetry)
            {
                // It's perfectly okay to not wait on the task here
                _sender.Enqueue(envelope);
            }
        }

        public void MarkSuccess()
        {
            _failureCount = 0;
            _sender.Unlatch();
            _pinger?.Dispose();
            _pinger = null;
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();

        public void Dispose()
        {
            // Doesn't own the sender
            _pinger?.Dispose();
        }
    }
}
