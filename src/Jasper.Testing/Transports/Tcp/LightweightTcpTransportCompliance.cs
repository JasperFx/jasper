using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    public class Sender : JasperOptions
    {
        public Sender()
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:2289/incoming".ToUri());
        }
    }

    public class Receiver : JasperOptions
    {
        public Receiver()
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:2288/incoming".ToUri());
        }
    }

    [Collection("compliance")]
    public class LightweightTcpTransportCompliance : SendingCompliance
    {
        public LightweightTcpTransportCompliance() : base($"tcp://localhost:2288/incoming".ToUri())
        {
            SenderIs<Sender>();

            ReceiverIs<Receiver>();
        }
    }
}
