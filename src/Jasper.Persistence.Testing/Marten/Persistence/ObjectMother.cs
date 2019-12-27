using Jasper.Transports;
using NSubstitute;

namespace Jasper.Persistence.Testing.Marten.Persistence
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
