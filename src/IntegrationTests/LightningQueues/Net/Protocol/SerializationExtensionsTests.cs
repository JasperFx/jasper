using System.Linq;
using System.Text;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Queues.Net.Protocol
{
    public class SerializationExtensionsTests
    {
        [Fact]
        public void can_serialize_and_deserialize()
        {
            var expected = new Envelope
            {
                Data = Encoding.UTF8.GetBytes("hello"),
                Id = MessageId.GenerateRandom(),
                Queue = "queue",
            };
            var messagesBytes = new[] {expected}.Serialize();
            var actual = messagesBytes.ToMessages().First();

            actual.Queue.ShouldBe(expected.Queue);
            actual.Data.ShouldBe(expected.Data);
            actual.Id.ShouldBe(expected.Id);
        }
    }
}
