using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using NSubstitute;

namespace Jasper.Testing.Bus
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
                Destination = TransportConstants.DelayedUri
            };
        }
    }
}
