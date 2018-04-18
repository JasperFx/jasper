using System;
using System.Threading.Tasks;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
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

        public Uri Destination => _sender.Destination;

        public async Task MarkFailed(OutgoingMessageBatch batch)
        {
            // If it's already latched, just enqueue again
            if (_sender.Latched)
            {
                await EnqueueForRetry(batch);
                return;
            }

            _failureCount++;

            if (_failureCount >= _settings.FailuresBeforeCircuitBreaks)
            {
                await _sender.LatchAndDrain();
                await EnqueueForRetry(batch);
                _pinger = new Pinger(_sender, _settings.Cooldown, restartSending);
            }
            else
            {
                foreach (var envelope in batch.Messages)
                {
#pragma warning disable 4014
                    _sender.Enqueue(envelope);
#pragma warning restore 4014
                }
            }


        }

        public abstract Task EnqueueForRetry(OutgoingMessageBatch batch);

        private Task restartSending()
        {
            _pinger.Dispose();
            _pinger = null;

            _sender.Unlatch();

            return afterRestarting(_sender);
        }

        protected abstract Task afterRestarting(ISender sender);

        public Task MarkSuccess()
        {
            _failureCount = 0;
            _sender.Unlatch();
            _pinger?.Dispose();
            _pinger = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Doesn't own the sender
            _pinger?.Dispose();
        }
    }
}
