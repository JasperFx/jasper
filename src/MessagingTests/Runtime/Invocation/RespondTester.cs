using System;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Util;
using Shouldly;
using TestMessages;
using Xunit;

namespace MessagingTests.Runtime.Invocation
{
    public class RespondTester
    {
        public readonly Envelope Original = new Envelope
        {
            ReplyUri = "tcp://server4:3333".ToUri()
        };

        [Fact]
        public void alter_any_which_way()
        {
            var toSend = Respond.With(new Message1()).Altered(e => e.AckRequested = true)
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.AckRequested.ShouldBeTrue();
        }

        [Fact]
        public void carries_the_message_forward()
        {
            var message2 = new Message2();
            var toSend = Respond.With(message2).ToSender()
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.Message.ShouldBe(message2);
        }

        [Fact]
        public void delayed_by()
        {
            var toSend = Respond.With(new Message1()).DelayedBy(5.Minutes())
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.ExecutionTime.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void delayed_until()
        {
            var time = DateTime.UtcNow.Date.AddDays(2);

            var toSend = Respond.With(new Message1()).DelayedUntil(time)
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.ExecutionTime.ShouldBe(time);
        }

        [Fact]
        public void send_back_to_a_specific_location()
        {
            var destination = "tcp://server5:4444".ToUri();

            var message2 = new Message2();
            var toSend = Respond.With(message2).To(destination)
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.Destination.ShouldBe(destination);
        }

        [Fact]
        public void send_back_to_sender()
        {
            var message2 = new Message2();
            var toSend = Respond.With(message2).ToSender()
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.Destination.ShouldBe(Original.ReplyUri);
        }

        [Fact]
        public void with_header()
        {
            var toSend = Respond.With(new Message1()).WithHeader("foo", "bar")
                .As<ISendMyself>().CreateEnvelope(Original);

            toSend.Headers["foo"].ShouldBe("bar");
        }
    }
}
