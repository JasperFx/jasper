using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Transports.Tcp;
using NSubstitute;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public class RetryAgentFixture : Fixture, ISender
    {
        private readonly List<Envelope> _enqueued = new List<Envelope>();

        private readonly OutgoingMessageBatch[] batches = new OutgoingMessageBatch[10];
        private bool _latched;

        private bool _pingFails;
        private bool _unlatched;
        protected LightweightSendingAgent theRetryAgent;
        protected AdvancedSettings theSettings;
        private Endpoint _endpoint;

        public RetryAgentFixture()
        {
            Title = "Lightweight Retry Agent";
        }


        void IDisposable.Dispose()
        {
        }

        Task ISender.Send(Envelope envelope)
        {
            _enqueued.Add(envelope);
            return Task.CompletedTask;
        }

        Uri ISender.Destination { get; }

        bool Latched => _latched;

        Task LatchAndDrain()
        {
            _latched = true;
            return Task.CompletedTask;
        }

        void Unlatch()
        {
            _unlatched = true;
        }

        Task<bool> ISender.Ping(CancellationToken cancellationToken)
        {
            if (_pingFails) throw new TimeoutException();

            return Task.FromResult(true);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

        public override void SetUp()
        {
            _latched = false;
            _unlatched = false;
            _pingFails = false;

            theSettings = new AdvancedSettings(null);

            _endpoint = Substitute.For<Endpoint>();
            theRetryAgent =
                new LightweightSendingAgent(TransportLogger.Empty(), MessageLogger.Empty(), this, theSettings, _endpoint);

            for (var i = 0; i < batches.Length; i++) batches[i] = batchForEnvelopes(15);

            _enqueued.Clear();
        }

        public override void TearDown()
        {
            theRetryAgent?.Dispose();
        }

        protected Envelope expiredEnvelope()
        {
            return new Envelope
            {
                DeliverBy = DateTime.UtcNow.AddHours(-1),
                Data = new byte[] {1, 2, 3, 4}
            };
        }

        protected Envelope notExpiredEnvelope()
        {
            return new Envelope
            {
                Data = new byte[] {1, 2, 3, 4}
            };
        }

        protected OutgoingMessageBatch batchForEnvelopes(int notExpired, int expired = 0)
        {
            var list = new List<Envelope>();
            for (var i = 0; i < notExpired; i++) list.Add(notExpiredEnvelope());

            for (var i = 0; i < expired; i++) list.Add(expiredEnvelope());

            foreach (var envelope in list) envelope.Destination = TransportConstants.LocalUri;

            return new OutgoingMessageBatch(TransportConstants.LocalUri, list);
        }

        [FormatAs("Sending a batch of {count} envelopes fails in the sender")]
        public Task BatchFails(int count)
        {
            var batch = batchForEnvelopes(count);
            return theRetryAgent.MarkFailed(batch);
        }

        [FormatAs("Sending a batch of {count} envelopes total, with {expired} expired envelopes fails in the sender")]
        public Task BatchFailsWithExpired(int count, int expired)
        {
            var batch = batchForEnvelopes(count - expired, expired);
            return theRetryAgent.MarkFailed(batch);
        }

        [FormatAs("No queued envelopes are expired")]
        public bool NoQueuedEnvelopesAreExpired()
        {
            return theRetryAgent.Queued.All(x => !x.IsExpired());
        }

        [FormatAs("AdvancedSettings.FailuresBeforeCircuitBreaks is {count}")]
        public void FailuresBeforeCircuitBreaks(int count)
        {
            _endpoint.FailuresBeforeCircuitBreaks = count;
        }

        [FormatAs("AdvancedSettings.PingIntervalForCircuitResume = 50 ms")]
        public void CooldownIs50Ms()
        {
            _endpoint.PingIntervalForCircuitResume = 50.Milliseconds();
        }

        [FormatAs("AdvancedSettings.MaximumEnvelopeRetryStorage = {number}")]
        public void MaximumEnvelopeRetryStorage(int number)
        {
            _endpoint.MaximumEnvelopeRetryStorage = number;
        }

        [FormatAs("Batch {index} was MarkFailed()")]
        public Task MarkFailed(int index)
        {
            return theRetryAgent.MarkFailed(batches[index]);
        }

        [FormatAs("The RetryAgent was marked successful")]
        public void MarkSuccess()
        {
            theRetryAgent.MarkSuccess();
        }

        [FormatAs("The sender was not latched")]
        public bool TheSenderWasNotLatched()
        {
            return !_latched;
        }

        [FormatAs("The sender was latched")]
        public bool TheSenderWasLatched()
        {
            return _latched;
        }

        [FormatAs("Batch {index} was enqueued to the sender")]
        public bool TheBatchWasQueued(int index)
        {
            var batch = batches[index];
            return batch.Messages.All(x => _enqueued.Contains(x));
        }

        [FormatAs("Wait for the queued count to be zero")]
        public async Task WaitForQueuedToBeZero()
        {
            while (theRetryAgent.Queued.Any()) await Task.Delay(50);
        }

        [FormatAs("The Sender was unlatched")]
        public bool TheSenderWasUnlatched()
        {
            return _unlatched;
        }

        [FormatAs("The queued envelope count in the sender should be {count}")]
        public int QueuedCount()
        {
            return theRetryAgent.Queued.Count;
        }

        [FormatAs("Pinging the sender is failing")]
        public void PingingIsFailing()
        {
            _pingFails = true;
        }

        [FormatAs("Pinging the sender is succeeding")]
        public void PingingIsSucceeding()
        {
            _pingFails = false;
        }
    }
}
