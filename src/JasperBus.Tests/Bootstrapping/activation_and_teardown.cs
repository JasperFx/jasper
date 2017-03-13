using System.Linq;
using JasperBus.Runtime;
using JasperBus.Tests.Stubs;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {
        [Fact]
        public void transport_is_disposed()
        {
            var transport = theRuntime.Container.GetAllInstances<ITransport>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            theRuntime.Dispose();

            transport.WasDisposed.ShouldBeTrue();
        }

        [Fact]
        public void channels_are_all_built_out()
        {
            theRegistry.ListenForMessagesFrom(Uri1);
            theRegistry.ListenForMessagesFrom(Uri2);
            theRegistry.ListenForMessagesFrom(Uri3);

            theChannels[Uri1].Channel.ShouldBeOfType<StubChannel>();
            theChannels[Uri2].Channel.ShouldBeOfType<StubChannel>();
            theChannels[Uri3].Channel.ShouldBeOfType<StubChannel>();

            
        }

        [Fact]
        public void channels_are_all_shutdown_on_dispose()
        {
            theRegistry.ListenForMessagesFrom(Uri1);
            theRegistry.ListenForMessagesFrom(Uri2);
            theRegistry.ListenForMessagesFrom(Uri3);

            var channel1 = theChannels[Uri1].Channel.ShouldBeOfType<StubChannel>();
            var channel2 = theChannels[Uri2].Channel.ShouldBeOfType<StubChannel>();
            var channel3 = theChannels[Uri3].Channel.ShouldBeOfType<StubChannel>();

            theRuntime.Dispose();

            channel1.WasDisposed.ShouldBeTrue();
            channel2.WasDisposed.ShouldBeTrue();
            channel3.WasDisposed.ShouldBeTrue();
        }
    }
}