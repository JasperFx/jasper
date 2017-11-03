using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class BusSettingsTests
    {
        private readonly BusSettings theSettings = new BusSettings();

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
        public void SendTo_is_cached()
        {
            var subscriber1 = theSettings.SendTo("tcp://localhost:2299/one");
            var subscriber2 = theSettings.SendTo("tcp://localhost:2299/one");

            subscriber1.ShouldBeSameAs(subscriber2);
        }


        [Fact]
        public async Task applies_lookups_to_senders()
        {
            theSettings.SendTo("fake://one");

            var lookups = new UriAliasLookup(new IUriLookup[0]);

            lookups.SetAlias("fake://one", "tcp://server:2222");

            await theSettings.ApplyLookups(lookups);

            theSettings.KnownSubscribers.Single()
                .Uri.ShouldBe("tcp://server:2222".ToUri());


        }

    }
}
