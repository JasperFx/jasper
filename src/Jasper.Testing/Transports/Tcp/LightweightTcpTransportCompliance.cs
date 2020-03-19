using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    public class Sender : JasperOptions
    {

    }

    public class Receiver : JasperOptions
    {
        public Receiver()
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:2288/incoming".ToUri());
        }
    }

    [Collection("integration")]
    public class LightweightTcpTransportCompliance : SendingCompliance
    {
        private static int port = 2114;

        public LightweightTcpTransportCompliance() : base($"tcp://localhost:2288/incoming".ToUri())
        {
            SenderIs<Sender>();

            ReceiverIs<Receiver>();
        }
    }
}
