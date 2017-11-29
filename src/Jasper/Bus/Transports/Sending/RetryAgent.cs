using System;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public abstract class RetryAgent : IDisposable
    {
        protected readonly ISender _sender;
        private int _failureCount = 0;
        private Pinger _pinger;
        protected readonly RetrySettings _settings;

        public RetryAgent(ISender sender, RetrySettings settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public void MarkFailed(OutgoingMessageBatch batch)
        {
            // If it's already latched, just enqueue again
            if (_sender.Latched)
            {
                EnqueueForRetry(batch);
                return;
            }

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

        public abstract void EnqueueForRetry(OutgoingMessageBatch batch);

        private void restartSending()
        {
            _pinger.Dispose();
            _pinger = null;

            _sender.Unlatch();

            afterRestarting();
        }

        protected abstract void afterRestarting();

        public void MarkSuccess()
        {
            _failureCount = 0;
            _sender.Unlatch();
            _pinger?.Dispose();
            _pinger = null;
        }

        public void Dispose()
        {
            // Doesn't own the sender
            _pinger?.Dispose();
        }
    }
}
