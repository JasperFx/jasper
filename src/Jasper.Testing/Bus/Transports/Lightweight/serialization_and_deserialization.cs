using System;
using System.Linq;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.Lightweight
{
    public class serialization_and_deserialization_of_single_message
    {
        private OutgoingMessage outgoing;
        private Message incoming;

        public serialization_and_deserialization_of_single_message()
        {
            outgoing = new OutgoingMessage
            {
                SentAt = DateTime.Today.ToUniversalTime(),
                Id = MessageId.GenerateRandom(),
                Queue = "incoming",
                SubQueue = "subqueue",
                Data = new byte[]{1, 5, 6, 11, 2, 3},
                Destination = "lq.tcp://localhost:2222/outgoing".ToUri(),

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
            incoming.Headers.Count.ShouldBe(3);
            incoming.Headers["name"].ShouldBe("Jeremy");
            incoming.Headers["state"].ShouldBe("Texas");
            incoming.Headers["reply-uri"].ShouldBe("lq.tcp://localhost:2221/replies");
        }


    }
}
