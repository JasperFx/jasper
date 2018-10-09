using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class MessagingSettingsTests
    {
        private readonly MessagingSettings theSettings = new MessagingSettings();

        [Fact]
        public void derive_the_node_id()
        {
            theSettings.MachineName = "SomeMachine";
            theSettings.ServiceName = "MyService";

            theSettings.NodeId.ShouldBe("MyService@SomeMachine");

            theSettings.MachineName = "OtherMachine";
            theSettings.NodeId.ShouldBe("MyService@OtherMachine");

            theSettings.ServiceName = "OtherService";
            theSettings.NodeId.ShouldBe("OtherService@OtherMachine");
        }


        [Fact]
        public void enable_and_disable_transport()
        {
            theSettings.StateFor("tcp").ShouldBe(TransportState.Enabled);

            theSettings.DisableTransport("tcp");
            theSettings.StateFor("tcp").ShouldBe(TransportState.Disabled);

            theSettings.EnableTransport("tcp");
            theSettings.StateFor("tcp").ShouldBe(TransportState.Enabled);
        }

    }
}
