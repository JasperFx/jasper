using System;
using System.Linq;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.Lightweight
{
    public class serialization_and_deserialization_of_single_message
    {
        private Envelope outgoing;
        private Envelope incoming;

        public serialization_and_deserialization_of_single_message()
        {
            outgoing = new Envelope
            {
                SentAt = DateTime.Today.ToUniversalTime(),
                Id = MessageId.GenerateRandom(),
                Queue = "incoming",
                SubQueue = "subqueue",
                Data = new byte[]{1, 5, 6, 11, 2, 3},
                Destination = "lq.tcp://localhost:2222/incoming".ToUri(),
                MaxAttempts = 3,
                SentAttempts = 2,
                DeliverBy = DateTime.Today.ToUniversalTime()
            };

            outgoing.Headers.Add("name", "Jeremy");
            outgoing.Headers.Add("state", "Texas");
            outgoing.Headers.Add("reply-uri", "lq.tcp://localhost:2221/replies");

            var messageBytes = new []{outgoing}.Serialize();
            incoming = messageBytes.ToMessages().First();
        }

        [Fact]
        public void brings_over_the_id()
        {
            incoming.Id.ShouldBe(outgoing.Id);
        }

        [Fact]
        public void queue()
        {
            incoming.Queue.ShouldBe(outgoing.Queue);
        }

        [Fact]
        public void subqueue()
        {
            incoming.SubQueue.ShouldBe(outgoing.SubQueue);
        }

        [Fact]
        public void sent_at()
        {
            incoming.SentAt.ShouldBe(outgoing.SentAt);
        }

        [Fact]
        public void data_comes_over()
        {
            incoming.Data.ShouldHaveTheSameElementsAs(outgoing.Data);
        }

        [Fact]
        public void all_the_headers()
        {
            incoming.Headers["name"].ShouldBe("Jeremy");
            incoming.Headers["state"].ShouldBe("Texas");
            incoming.Headers["reply-uri"].ShouldBe("lq.tcp://localhost:2221/replies");
        }

        [Fact]
        public void destination()
        {
            incoming.Destination.ShouldBe(outgoing.Destination);
        }

        [Fact]
        public void max_attempts()
        {
            incoming.MaxAttempts.ShouldBe(outgoing.MaxAttempts);
        }

        [Fact]
        public void sent_attempts()
        {
            incoming.SentAttempts.ShouldBe(outgoing.SentAttempts);
        }


        [Fact]
        public void deliver_by()
        {
            incoming.DeliverBy.Value.ShouldBe(outgoing.DeliverBy.Value);
        }

    }
}
