using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class default_channel_configuration
    {
        [Fact]
        public void use_the_loopback_replies_queue_by_default()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.ConventionalDiscoveryDisabled = true;
            }))
            {

                var channels = runtime.Get<IChannelGraph>().As<ChannelGraph>();
                channels.DefaultChannel
                    .ShouldBeTheSameAs(channels.IncomingChannelsFor("loopback").Single());
            }
        }

        [Fact]
        public void override_the_default_channel()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.ConventionalDiscoveryDisabled = true;
                _.Channels.DefaultIs("loopback://incoming");
            }))
            {
                var channels = runtime.Get<IChannelGraph>().As<ChannelGraph>();
                channels.DefaultChannel
                    .ShouldBeTheSameAs(channels["loopback://incoming"]);
            }
        }


        [Fact]
        public async Task will_route_to_the_default_channel_if_there_is_a_handler_but_no_routes()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.ConventionalDiscoveryDisabled = true;
                _.Channels.DefaultIs("loopback://incoming");
                _.Handlers.IncludeType<DefaultRoutedMessageHandler>();
            }))
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Single().Destination.ShouldBe("loopback://incoming".ToUri());
            }
        }

        [Fact]
        public async Task will_not_route_to_the_default_channel_if_there_is_a_handler_and_routes()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.ConventionalDiscoveryDisabled = true;
                _.Channels.DefaultIs("loopback://incoming");
                _.Handlers.IncludeType<DefaultRoutedMessageHandler>();

                _.Messaging.Send<DefaultRoutedMessage>().To("jasper://localhost:2444/outgoing");
            }))
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Single().Destination.ShouldBe("jasper://localhost:2444/outgoing".ToUri());
            }
        }

        [Fact]
        public async Task will_not_route_locally_with_no_handler()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.ConventionalDiscoveryDisabled = true;
                _.Channels.DefaultIs("loopback://incoming");

            }))
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Any().ShouldBeFalse();
            }
        }


    }

    public class DefaultRoutedMessage
    {

    }

    public class DefaultRoutedMessageHandler
    {
        public void Handle(DefaultRoutedMessage message)
        {

        }
    }
}
