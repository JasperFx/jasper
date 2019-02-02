using Jasper;
using Shouldly;
using Xunit;

namespace CoreTests
{
    public class JasperOptionsTests
    {
        private readonly JasperOptions theSettings = new JasperOptions();

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
