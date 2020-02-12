using Jasper.Transports;
using NSubstitute;

namespace Jasper.Testing.Messaging
{
    public static class ObjectMother
    {
        public static Envelope Envelope()
        {
            return new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                Callback = Substitute.For<IFullMessageCallback>(),
                MessageType = "Something",
                Destination = TransportConstants.ScheduledUri
            };
        }
    }
}
