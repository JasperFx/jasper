using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.Sending
{


    public class do_not_latch_on_failures_if_they_are_not_consecutive: LightweightRetryAgentContext
    {
        private OutgoingMessageBatch[] batches = new OutgoingMessageBatch[10];

        public do_not_latch_on_failures_if_they_are_not_consecutive()
        {
            theSettings.FailuresBeforeCircuitBreaks = 3;

            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = batchForEnvelopes(15);
            }

            theRetryAgent.MarkFailed(batches[0]);
            theRetryAgent.MarkFailed(batches[1]);
            theRetryAgent.MarkSuccess();
            theRetryAgent.MarkFailed(batches[2]);
            theRetryAgent.MarkFailed(batches[3]);
            theRetryAgent.MarkSuccess();
        }

        [Fact]
        public void should_never_latch()
        {
            theSender.DidNotReceive().Latch();
        }

        [Fact]
        public void all_failed_batches_are_requeued()
        {
            allEnvelopesWereEnqueuedToTheSender(batches[0]);
            allEnvelopesWereEnqueuedToTheSender(batches[1]);
            allEnvelopesWereEnqueuedToTheSender(batches[2]);
            allEnvelopesWereEnqueuedToTheSender(batches[3]);
        }
    }


    public class will_unlatch_and_resend_after_the_other_node_comes_back_online : LightweightRetryAgentContext
    {
        private OutgoingMessageBatch batch1;
        private OutgoingMessageBatch batch2;
        private OutgoingMessageBatch batch3;

        public will_unlatch_and_resend_after_the_other_node_comes_back_online()
        {
            Func<CallInfo, Task> badPing = x => {throw new TimeoutException();};
            theSender.Ping().Returns(badPing, badPing, badPing, badPing, badPing, badPing, x => Task.CompletedTask);


            theSettings.Cooldown = 50.Milliseconds();

            for (int i = 0; i < 4; i++)
            {
                theRetryAgent.MarkFailed(batchForEnvelopes(5));
            }

            batch1 = batchForEnvelopes(10);
            batch2 = batchForEnvelopes(10);
            batch3 = batchForEnvelopes(10);

            theRetryAgent.MarkFailed(batch1);
            theRetryAgent.MarkFailed(batch2);
            theRetryAgent.MarkFailed(batch3);

            waitForQueuedToBeZero().Wait(3.Seconds());
        }

        private async Task waitForQueuedToBeZero()
        {
            while (theRetryAgent.Queued.Any())
            {
                await Task.Delay(50);
            }
        }

        [Fact]
        public void should_unlatch_the_sender()
        {
            theSender.Received().Unlatch();
        }

        //[Fact] does not play well on CI in a batch
        public void should_have_requeued_all_envelopes()
        {
            allEnvelopesWereEnqueuedToTheSender(batch1);
            allEnvelopesWereEnqueuedToTheSender(batch2);
            allEnvelopesWereEnqueuedToTheSender(batch3);

        }

        [Fact]
        public void no_longer_has_queued_envelopes()
        {
            theRetryAgent.Queued.Any().ShouldBeFalse();
        }
    }



    public class only_keep_the_retry_storage_limit_when_getting_failures : LightweightRetryAgentContext
    {
        public only_keep_the_retry_storage_limit_when_getting_failures()
        {
            theSettings.MaximumEnvelopeRetryStorage = 200;

            // Can never be cleared
            theSender.Ping().Throws(new TimeoutException());



            for (int i = 0; i < 10; i++)
            {
                theRetryAgent.MarkFailed(batchForEnvelopes(35, 100));
            }
        }

        [Fact]
        public void only_keeps_the_most_recent()
        {
            theRetryAgent.Queued.Count.ShouldBe(theSettings.MaximumEnvelopeRetryStorage);
        }
    }


    public class clear_out_expired_envelopes_when_exceeding_queued_count : LightweightRetryAgentContext
    {
        public clear_out_expired_envelopes_when_exceeding_queued_count()
        {
            theSettings.MaximumEnvelopeRetryStorage = 200;

            // Can never be cleared
            theSender.Ping().Throws(new TimeoutException());



            for (int i = 0; i < 10; i++)
            {
                theRetryAgent.MarkFailed(batchForEnvelopes(25, 100));
            }
        }

        [Fact]
        public void does_not_requeue_any_expired_envelopes()
        {
            theRetryAgent.Queued.Any().ShouldBeTrue();

            theRetryAgent.Queued.Any(x => x.IsExpired()).ShouldBeFalse();
        }
    }


    public class latch_when_it_receives_too_many_failures : LightweightRetryAgentContext
    {
        private OutgoingMessageBatch batch1;
        private OutgoingMessageBatch batch2;
        private OutgoingMessageBatch batch3;
        private OutgoingMessageBatch batch4;

        public latch_when_it_receives_too_many_failures()
        {
            batch1 = batchForEnvelopes(5);
            batch2 = batchForEnvelopes(5);
            batch3 = batchForEnvelopes(5);
            batch4 = batchForEnvelopes(5);

            // Can never be cleared
            theSender.Ping().Throws(new TimeoutException());

            theRetryAgent.MarkFailed(batch1);
            theRetryAgent.MarkFailed(batch2);
            theRetryAgent.MarkFailed(batch3);
            theRetryAgent.MarkFailed(batch4);
        }

        [Fact]
        public void should_latch_the_sender()
        {
            theSender.Received().Latch();
        }

        [Fact]
        public void should_be_queueing_up_batch_envelopes_for_later()
        {
            theRetryAgent.Queued.Any().ShouldBeTrue();
        }
    }


    public class when_getting_failed_batches_but_below_the_failure_threshold : LightweightRetryAgentContext
    {
        private OutgoingMessageBatch batch1;
        private OutgoingMessageBatch batch2;

        public when_getting_failed_batches_but_below_the_failure_threshold()
        {
            theSettings.FailuresBeforeCircuitBreaks = 3;

            batch1 = batchForEnvelopes(5);
            batch2 = batchForEnvelopes(5);

            theRetryAgent.MarkFailed(batch1);
            theRetryAgent.MarkFailed(batch2);
        }

        [Fact]
        public void should_not_have_latched_the_sender()
        {
            theSender.DidNotReceive().Latch();
        }

        [Fact]
        public void should_requeue_all_messages()
        {
            foreach (var envelope in batch1.Messages)
            {
                theSender.Received().Enqueue(envelope);
            }

            foreach (var envelope in batch2.Messages)
            {
                theSender.Received().Enqueue(envelope);
            }
        }

        [Fact]
        public void should_not_be_keeping_any_envelopes_for_resending()
        {
            theRetryAgent.Queued.Any().ShouldBeFalse();
        }
    }


    public abstract class LightweightRetryAgentContext : IDisposable
    {
        protected readonly ISender theSender = Substitute.For<ISender>();
        protected readonly RetrySettings theSettings = new RetrySettings();
        protected LightweightRetryAgent theRetryAgent;

        protected Envelope expiredEnvelope()
        {
            return new Envelope
            {
                DeliverBy = DateTime.UtcNow.AddHours(-1),
                Data = new byte[]{1,2,3,4}
            };
        }

        protected Envelope notExpiredEnvelope()
        {
            return new Envelope
            {
                Data = new byte[]{1,2,3,4}
            };
        }

        protected OutgoingMessageBatch batchForEnvelopes(int notExpired, int expired = 0)
        {
            var list = new List<Envelope>();
            for (int i = 0; i < notExpired; i++)
            {
                list.Add(notExpiredEnvelope());
            }

            for (int i = 0; i < expired; i++)
            {
                list.Add(expiredEnvelope());
            }

            foreach (var envelope in list)
            {
                envelope.Destination = TransportConstants.LoopbackUri;
            }

            return new OutgoingMessageBatch(TransportConstants.LoopbackUri, list);
        }

        public LightweightRetryAgentContext()
        {
            theRetryAgent = new LightweightRetryAgent(theSender, theSettings);
        }


        public void Dispose()
        {
            theSender?.Dispose();
            theRetryAgent?.Dispose();
        }


        protected void allEnvelopesWereEnqueuedToTheSender(OutgoingMessageBatch batch)
        {
            foreach (var envelope in batch.Messages)
            {
                theSender.Received().Enqueue(envelope);
            }
        }
    }
}
