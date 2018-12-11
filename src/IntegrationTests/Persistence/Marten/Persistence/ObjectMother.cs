using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using NSubstitute;

namespace IntegrationTests.Persistence.Marten.Persistence
{
    public static class ObjectMother
    {
        public static Envelope Envelope()
        {
            return new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                Callback = Substitute.For<IMessageCallback>(),
                MessageType = "Something",
                Destination = TransportConstants.ScheduledUri
            };
        }
    }
}
